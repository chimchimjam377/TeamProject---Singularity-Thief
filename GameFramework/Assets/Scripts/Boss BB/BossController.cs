using System.Collections;
using UnityEngine;

public class BossController : MonoBehaviour
{
    public enum BossState { Phase1, ChargingLaser, Stunned, Phase2, Dead, Transition }
    public BossState State { get; private set; } = BossState.Phase1;
    [Header("References")]
    [SerializeField] private PlayerStats player;
    [SerializeField] private Transform leftArm, rightArm, firePoint, phase2BossPoint;
    [SerializeField] private GameObject phase2Platforms, coreClosedVisual, coreOpenVisual;
    [SerializeField] private GameObject laserWarning, laserBlast;
    [Header("V7 Arena laser")]
    [SerializeField] private Transform laserRisePoint;
    [SerializeField] private Camera arenaCamera;
    [SerializeField] private float laserRiseTime = 1.2f, laserSkyChargeTime = 1.15f;
    [SerializeField] private float laserBeamDuration = 1.3f, laserReturnTime = 1.45f;
    [SerializeField] private float laserBeamTravelTime = 0.16f, laserFadeTime = 0.45f;
    [SerializeField] private float laserOffscreenMargin = 0.75f, laserFallbackRise = 20f;
    [SerializeField] private float chainStruggleTime = 0.65f, chainStruggleHeight = 0.35f;
    public bool LaserWarningVisible { get; private set; }
    public bool LaserBeamVisible { get; private set; }
    public float LaserBeamDepth { get; private set; }
    public float LaserBeamOpacity { get; private set; }
    public Camera ArenaCamera => arenaCamera;
    public bool UseAutomaticWarning => !laserWarning;
    public bool UseAutomaticBeam => !laserBlast;
    [Header("Phase 1 (test values)")]
    [SerializeField] private int phase1HP = 300;
    [SerializeField] private int laserThreshold = 180, phase2Threshold = 80;
    [SerializeField] private float laserChargeTime = 12f, stunTime = 8f, laserRetryDelay = 5f;
    [SerializeField] private int overloadDamage = 30;
    [Header("Phase 2 (test values)")]
    [SerializeField] private int coreHP = 180, bombsToOpenCore = 3;
    [SerializeField] private float coreOpenTime = 10f;
    [Header("Attack prefabs / drop points")]
    [SerializeField] private BossBomb bombPrefab;
    [SerializeField] private BossHomingBullet homingPrefab;
    [SerializeField] private Transform leftDudDropPoint, rightDudDropPoint, phase2DudDropPoint;
    [SerializeField] private float bombDropHeight = 9f, attackInterval = 1.2f;
    [Header("Mine rain (world-space left/right spawn markers)")]
    [SerializeField] private Transform rainLeft, rainRight;
    [SerializeField] private Transform mineLaunchPoint;
    [SerializeField] private float mineLaunchRise = 3.5f, mineLaunchGravity = 16f;
    [SerializeField] private int rainLanes = 9, minesPerWave = 5, rainWaves = 4;
    [SerializeField] private float rainWaveInterval = 0.55f, rainSpawnInterval = 0.065f;
    [SerializeField] private float rainFallbackWidth = 16f;
    [SerializeField] private int wavesPerDud = 2;
    private int rainWaveCounter;
    [Header("Arm attack (test values)")]
    [SerializeField] private int armDamage = 20;
    [Header("Animation")]
    [SerializeField] private float armReturnTime = 1f;
    [SerializeField] private float bodyWindupTime = 0.75f, bodyDropTime = 0.28f;
    [SerializeField] private float sweepTravelTime = 0.75f, sweepWindupHeight = 2f;
    [SerializeField] private float phase2RiseTime = 1.5f;
    [SerializeField] private float armHitRadius = 2f, sweepWorldY = 0f;
    [Header("Natural motion (optional shoulder references)")]
    [SerializeField] private Transform leftShoulder, rightShoulder;
    [SerializeField] private float hoverHeight = 0.15f, hoverFrequency = 0.65f;
    [SerializeField] private float idleArmAngle = 3f, windupArmAngle = 16f;
    [SerializeField] private float impactHold = 0.10f, recoveryLift = 1.4f;
    [SerializeField] private float stunnedDrop = 0.3f;
    [Header("Full-arm sweep")]
    [SerializeField] private float sweepArmRadius = 0.55f;
    [SerializeField] private float sweepBodyTravel = 1f;
    [SerializeField] private float sweepWindbackAngle = 55f, sweepFollowThroughAngle = 78f;
    [SerializeField] private float sweepPoseHold = 0.22f;
    [SerializeField] private float sweepMaxLowering = 3.5f, sweepBodyClearance = 1f;
    private BossMechaArmRig leftRig, rightRig;
    private float leftAngle, rightAngle;
    private Vector3 phase2Home;
    private int hp, currentCoreHP, chains, coreBombHits, dudCounter;
    private bool hasOverloaded, coreOpen;
    private float nextLaserTime, coreCloseTime;
    private Vector3 leftHome, rightHome, bodyHome;
    private bool bodyAttackActive;
    private BossBombCarrier carrier;
    public int CurrentHP => State == BossState.Phase2 || State == BossState.Dead ? currentCoreHP : hp;
    public int ChainCount => chains;
    public bool CoreOpen => coreOpen;
    public bool BodyAttackActive => bodyAttackActive;
    private bool PlayerAlive => player != null && !player.IsDead && player.gameObject.activeInHierarchy;
    private bool Attacking => PlayerAlive && (State == BossState.Phase1 || State == BossState.Phase2);

    private void Start()
    {
        bodyHome = transform.position;
        if (!arenaCamera) arenaCamera = Camera.main;
        if ((!laserWarning || !laserBlast) && !GetComponent<BossArenaLaserEffect>())
            gameObject.AddComponent<BossArenaLaserEffect>();
        hp = Mathf.Max(1, phase1HP);
        currentCoreHP = Mathf.Max(1, coreHP);
        if (player == null) player = FindFirstObjectByType<PlayerStats>();
        if (player == null) { Debug.LogError("Boss: PlayerStats missing", this); enabled = false; return; }
        carrier = player.GetComponent<BossBombCarrier>();
        if (leftArm) leftHome = leftArm.localPosition;
        if (rightArm) rightHome = rightArm.localPosition;
        foreach (var rig in GetComponentsInChildren<BossMechaArmRig>(true))
        {
            if (rig.HandTarget == leftArm) { leftRig = rig; if (!leftShoulder) leftShoulder = rig.ShoulderPivot; }
            if (rig.HandTarget == rightArm) { rightRig = rig; if (!rightShoulder) rightShoulder = rig.ShoulderPivot; }
        }
        // Calibrate before any attack changes the neutral pose.
        if (leftRig && leftArm) leftRig.TryGetPose(leftArm.position, out _, out _, out _);
        if (rightRig && rightArm) rightRig.TryGetPose(rightArm.position, out _, out _, out _);
        SetActive(phase2Platforms, false);
        SetActive(laserWarning, false);
        SetActive(laserBlast, false);
        SetActive(coreClosedVisual, true);
        SetActive(coreOpenVisual, false);
        StartCoroutine(AttackLoop());
    }

    private void Update()
    {
        if (!PlayerAlive) return;
        UpdateHover();
        if (State == BossState.Phase1 && !bodyAttackActive && hp <= laserThreshold && Time.time >= nextLaserTime)
            StartCoroutine(LaserRoutine());
        if (State == BossState.Phase2 && coreOpen && Time.time >= coreCloseTime)
        {
            coreOpen = false;
            coreBombHits = 0;
            SetActive(coreClosedVisual, true);
            SetActive(coreOpenVisual, false);
            Debug.Log("Core closed");
        }
    }

    public void TakePartDamage(int damage, bool isCore)
    {
        if (damage <= 0 || !PlayerAlive) return;
        if (State == BossState.Phase2)
        {
            if (!isCore || !coreOpen) return;
            currentCoreHP = Mathf.Max(0, currentCoreHP - damage);
            Debug.Log($"Core HP: {currentCoreHP}/{coreHP}");
            if (currentCoreHP == 0) Defeat();
            return;
        }
        if (isCore || State == BossState.Dead || State == BossState.ChargingLaser || State == BossState.Transition) return;
        if (State == BossState.Phase1 && !hasOverloaded)
        {
            hp = Mathf.Max(laserThreshold, hp - damage);
        }
        else if (State == BossState.Stunned)
        {
            hp = Mathf.Max(0, hp - damage);
            if (hp <= phase2Threshold) EnterPhase2();
        }
    }

    public bool AddChain()
    {
        if ((State != BossState.Phase1 && State != BossState.ChargingLaser) || chains >= 2) return false;
        chains++;
        Debug.Log($"Boss chains: {chains}/2");
        return true;
    }

    public void HitCoreWithBomb()
    {
        if (State != BossState.Phase2 || coreOpen) return;
        coreBombHits++;
        Debug.Log($"Core armor: {coreBombHits}/{bombsToOpenCore}");
        if (coreBombHits >= Mathf.Max(1, bombsToOpenCore))
        {
            coreOpen = true;
            coreCloseTime = Time.time + coreOpenTime;
            SetActive(coreClosedVisual, false);
            SetActive(coreOpenVisual, true);
            Debug.Log("Core OPEN for " + coreOpenTime + " seconds");
        }
    }

    private IEnumerator LaserRoutine()
    {
        State = BossState.ChargingLaser;
        bodyAttackActive = true;
        if (carrier) carrier.EndGrab();
        ResetArms();
        ShowLaserWarning(true);
        // A ground-level preparation window supplies duds before the ascent.
        float end = Time.time + Mathf.Max(0f, laserChargeTime);
        float nextBomb = Time.time;
        while (PlayerAlive && chains < 2 && Time.time < end)
        {
            if (Time.time >= nextBomb)
            {
                SpawnDud();
                SpawnRainMine(0); SpawnRainMine(2); SpawnRainMine(4);
                nextBomb = Time.time + 2f;
            }
            yield return null;
        }
        if (!PlayerAlive) { AbortLaser(); yield break; }

        // With both chains attached the boss cannot leave the arena.
        if (chains < 2)
        {
            Vector3 from = transform.position;
            Vector3 sky = GetLaserSkyPosition();
            float duration = Mathf.Max(0.01f, laserRiseTime);
            for (float t = 0f; t < duration && PlayerAlive && chains < 2; t += Time.deltaTime)
            {
                float p = Mathf.Clamp01(t / duration);
                transform.position = Vector3.Lerp(from, sky, p * p);
                yield return null;
            }
            if (PlayerAlive && chains < 2)
            {
                transform.position = sky;
                float skyEnd = Time.time + Mathf.Max(0f, laserSkyChargeTime);
                while (PlayerAlive && chains < 2 && Time.time < skyEnd) yield return null;
            }
        }
        if (!PlayerAlive) { AbortLaser(); yield break; }
        ShowLaserWarning(false);
        if (chains >= 2)
        {
            // If the final chain was activated during the ascent, pull back to the tethered pose.
            Vector3 restrained = bodyHome + Vector3.up * Mathf.Max(0f, chainStruggleHeight);
            yield return LaserMove(restrained, 0.35f);
            float duration = Mathf.Max(0.01f, chainStruggleTime);
            for (float t = 0f; t < duration && PlayerAlive; t += Time.deltaTime)
            {
                float p = Mathf.Clamp01(t / duration);
                transform.position = restrained + Vector3.up * (Mathf.Sin(p * Mathf.PI * 5f) * 0.10f * (1f - p));
                yield return null;
            }
            if (!PlayerAlive) { AbortLaser(); yield break; }
            transform.position = bodyHome;
            bodyAttackActive = false;
            State = BossState.Stunned;
            hasOverloaded = true;
            hp = Mathf.Max(phase2Threshold + 1, hp - Mathf.Max(0, overloadDamage));
            Debug.Log("Boss ascent stopped by two chains / overloaded");
            yield return new WaitForSeconds(stunTime);
            if (State == BossState.Stunned)
            {
                State = BossState.Phase1;
                nextLaserTime = Time.time + laserRetryDelay;
            }
            yield break;
        }

        // Arena-wide damage is deliberately independent of collision, guard or dodge.
        LaserBeamVisible = true;
        LaserBeamOpacity = 1f;
        LaserBeamDepth = 0f;
        SetActive(laserBlast, true);
        float travel = Mathf.Max(0.01f, laserBeamTravelTime);
        for (float t = 0f; t < travel; t += Time.deltaTime)
        {
            LaserBeamDepth = Mathf.Clamp01(t / travel);
            yield return null;
        }
        LaserBeamDepth = 1f;
        if (PlayerAlive) player.TakeUnavoidableDamage(player.CurrentHealth);
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, laserBeamDuration));
        float fade = Mathf.Max(0.01f, laserFadeTime);
        for (float t = 0f; t < fade; t += Time.unscaledDeltaTime)
        {
            LaserBeamOpacity = 1f - Mathf.Clamp01(t / fade);
            yield return null;
        }
        HideLaserBeam();
        yield return LaserMove(bodyHome, laserReturnTime);
        bodyAttackActive = false;
        State = BossState.Phase1;
        nextLaserTime = Time.time + laserRetryDelay;
    }

    private Vector3 GetLaserSkyPosition()
    {
        Vector3 pos = transform.position;
        // A fixed arena camera is assumed. Use the lowest sprite edge so the entire robot clears the view.
        float lowest = pos.y;
        foreach (var sr in GetComponentsInChildren<SpriteRenderer>())
            if (sr.enabled) lowest = Mathf.Min(lowest, sr.bounds.min.y);
        float bottomOffset = pos.y - lowest;
        Vector3 sky = laserRisePoint ? laserRisePoint.position : bodyHome + Vector3.up * laserFallbackRise;
        if (arenaCamera && arenaCamera.orthographic)
        {
            float top = arenaCamera.transform.position.y + arenaCamera.orthographicSize;
            sky.y = Mathf.Max(sky.y, top + bottomOffset + Mathf.Max(0.1f, laserOffscreenMargin));
        }
        sky.z = bodyHome.z;
        return sky;
    }

    private IEnumerator LaserMove(Vector3 target, float duration)
    {
        Vector3 start = transform.position;
        duration = Mathf.Max(0.01f, duration);
        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            transform.position = Vector3.Lerp(start, target, Mathf.SmoothStep(0f, 1f, t / duration));
            yield return null;
        }
        transform.position = target;
    }

    private void ShowLaserWarning(bool visible)
    {
        LaserWarningVisible = visible;
        SetActive(laserWarning, visible);
    }

    private void HideLaserBeam()
    {
        LaserBeamVisible = false;
        LaserBeamOpacity = LaserBeamDepth = 0f;
        SetActive(laserBlast, false);
    }

    private void AbortLaser()
    {
        ShowLaserWarning(false);
        HideLaserBeam();
        bodyAttackActive = false;
        State = BossState.Phase1;
        transform.position = bodyHome;
    }

    private void EnterPhase2()
    {
        State = BossState.Transition;
        if (carrier) carrier.EndGrab();
        ResetArms();
        ShowLaserWarning(false);
        HideLaserBeam();
        foreach (var bomb in FindObjectsByType<BossBomb>(FindObjectsSortMode.None)) Destroy(bomb.gameObject);
        foreach (var bullet in FindObjectsByType<BossHomingBullet>(FindObjectsSortMode.None)) Destroy(bullet.gameObject);
        SetActive(phase2Platforms, true);
        if (leftArm) leftArm.gameObject.SetActive(false);
        if (rightArm) rightArm.gameObject.SetActive(false);
        StartCoroutine(RiseToPhase2());
    }

    private IEnumerator RiseToPhase2()
    {
        Vector3 start = transform.position;
        Vector3 end = phase2BossPoint ? phase2BossPoint.position : start;
        float duration = Mathf.Max(0.01f, phase2RiseTime);
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            if (State != BossState.Transition) yield break;
            transform.position = Vector3.Lerp(start, end, Mathf.SmoothStep(0f, 1f, t / duration));
            yield return null;
        }
        transform.position = end;
        phase2Home = end;
        State = BossState.Phase2;
        coreOpen = false;
        coreBombHits = 0;
        SetActive(coreClosedVisual, true);
        SetActive(coreOpenVisual, false);
        Debug.Log("PHASE 2");
    }

    private void Defeat()
    {
        State = BossState.Dead;
        coreOpen = false;
        if (carrier) carrier.EndGrab();
        StopAllCoroutines();
        ShowLaserWarning(false);
        HideLaserBeam();
        foreach (var bullet in FindObjectsByType<BossHomingBullet>(FindObjectsSortMode.None)) Destroy(bullet.gameObject);
        foreach (var bomb in FindObjectsByType<BossBomb>(FindObjectsSortMode.None)) Destroy(bomb.gameObject);
        Debug.Log("BOSS DEFEATED");
        gameObject.SetActive(false);
    }

    private IEnumerator AttackLoop()
    {
        int pattern = 0;
        while (State != BossState.Dead && PlayerAlive)
        {
            if (!Attacking) { yield return null; continue; }
            if (State == BossState.Phase2)
            {
                // Alternate so bombs continue to be available.
                if (pattern++ % 2 == 0) yield return BombVolley();
                else yield return HomingVolley();
            }
            else
            {
                switch (pattern++ % 5)
                {
                    case 0: yield return BombVolley(); break;
                    case 1: yield return HomingVolley(); break;
                    case 2: yield return Slam(leftArm); break;
                    case 3: yield return Sweep(); break;
                    case 4: yield return Grab(leftArm); break;
                }
            }
            yield return new WaitForSeconds(attackInterval);
        }
        if (carrier) carrier.EndGrab();
        ResetArms();
    }

    private IEnumerator BombVolley()
    {
        // Scatter across the arena, leaving two adjacent lanes empty per volley wave.
        for (int wave = 0; wave < Mathf.Max(1, rainWaves) && Attacking; wave++)
        {
            int lanes = Mathf.Max(3, rainLanes);
            int gap = Random.Range(0, lanes - 1);
            var choices = new System.Collections.Generic.List<int>();
            for (int i = 0; i < lanes; i++) if (i != gap && i != gap + 1) choices.Add(i);
            int count = Mathf.Min(Mathf.Max(1, minesPerWave), choices.Count);
            if (++rainWaveCounter % Mathf.Max(1, wavesPerDud) == 0 || (wave == 0 && State == BossState.Phase2))
                SpawnDud();
            for (int i = 0; i < count && Attacking; i++)
            {
                int pick = Random.Range(0, choices.Count);
                SpawnRainMine(choices[pick]); choices.RemoveAt(pick);
                yield return new WaitForSeconds(Mathf.Max(0.01f, rainSpawnInterval));
            }
            yield return new WaitForSeconds(Mathf.Max(0.01f, rainWaveInterval));
        }
    }

    private void SpawnRainMine(int lane)
    {
        if (!bombPrefab || !PlayerAlive) return;
        int lanes = Mathf.Max(3, rainLanes);
        float u = Mathf.Clamp01((Mathf.Clamp(lane, 0, lanes - 1) + 0.5f) / lanes);
        Vector3 a = rainLeft ? rainLeft.position : new Vector3(bodyHome.x - rainFallbackWidth * 0.5f,
            player.transform.position.y + bombDropHeight, bodyHome.z);
        Vector3 b = rainRight ? rainRight.position : new Vector3(bodyHome.x + rainFallbackWidth * 0.5f,
            player.transform.position.y + bombDropHeight, bodyHome.z);
        Vector3 pos = Vector3.Lerp(a, b, u);
        LaunchMine(pos, false);
    }

    private void SpawnDud()
    {
        if (!bombPrefab || !PlayerAlive) return;
        Transform point = State == BossState.Phase2 ? phase2DudDropPoint :
            (dudCounter++ % 2 == 0 ? leftDudDropPoint : rightDudDropPoint);
        Vector3 pos = point ? point.position : player.transform.position + Vector3.up * bombDropHeight;
        // Duds do not expire on the ground, so avoid flooding the same collection point.
        foreach (var existing in FindObjectsByType<BossBomb>(FindObjectsSortMode.None))
            if (existing.IsAvailableDud && Mathf.Abs(existing.IntendedLandingX - pos.x) < 0.45f) return;
        LaunchMine(pos, true);
    }

    private void LaunchMine(Vector3 guide, bool dud)
    {
        Vector3 origin = mineLaunchPoint ? mineLaunchPoint.position :
            (firePoint ? firePoint.position : transform.position + Vector3.up);
        Instantiate(bombPrefab, origin, Quaternion.identity)
            .InitializeLaunch(dud, player, guide, mineLaunchRise, mineLaunchGravity);
    }

    private IEnumerator HomingVolley()
    {
        for (int i = 0; i < 3 && Attacking; i++)
        {
            SpawnHoming();
            yield return new WaitForSeconds(0.35f);
        }
    }

    private void SpawnHoming()
    {
        if (!homingPrefab || !PlayerAlive) return;
        Vector3 pos = firePoint ? firePoint.position : transform.position;
        Instantiate(homingPrefab, pos, Quaternion.identity).Initialize(player);
    }

    private bool CanBodyAttack => Attacking && State == BossState.Phase1;

    // Calculate a ROOT position that brings the hand to the requested location.
    // The hand's local position never changes; the whole boss moves instead.
    private Vector3 BodyPositionForHand(Transform hand, Vector3 worldTarget)
    {
        Vector3 home = hand == leftArm ? leftHome : rightHome;
        Vector3 worldHome = hand.parent ? hand.parent.TransformPoint(home) : home;
        Vector3 offset = worldHome - transform.position;
        return new Vector3(worldTarget.x - offset.x, worldTarget.y - offset.y, bodyHome.z);
    }

    private IEnumerator Slam(Transform arm)
    {
        if (!arm || !CanBodyAttack) yield break;
        bodyAttackActive = true;
        Vector3 target = player.transform.position;
        Vector3 strike = BodyPositionForHand(arm, target);
        yield return MoveBody(strike + Vector3.up * Mathf.Max(0.5f, sweepWindupHeight), bodyWindupTime,
            arm == leftArm ? -windupArmAngle : 0f, arm == rightArm ? windupArmAngle : 0f);
        yield return new WaitForSeconds(0.35f);
        yield return MoveBody(strike, bodyDropTime, 0f, 0f, true);
        if (CanBodyAttack && InRange(arm)) player.TakeDamage(armDamage, arm.gameObject);
        yield return new WaitForSeconds(Mathf.Max(0f, impactHold));
        yield return new WaitForSeconds(0.15f);
        yield return ReturnBody();
        bodyAttackActive = false;
    }

    private IEnumerator Sweep()
    {
        if (!CanBodyAttack || !leftArm || !rightArm) yield break;
        bool right = player.transform.position.x >= bodyHome.x;
        Transform arm = right ? rightArm : leftArm;
        BossMechaArmRig rig = right ? rightRig : leftRig;
        if (!rig) { Debug.LogWarning("Sweep requires a configured arm rig", this); yield break; }
        float side = right ? 1f : -1f;
        // A planar arc around the shoulder: draw back, pass low across the body,
        // then follow through. The arm lengths are calibrated once and never stretch.
        Vector3 home = right ? rightHome : leftHome;
        Vector2 neutralHand = arm.parent.TransformPoint(home);
        if (!rig.TryGetPose(neutralHand, out Vector2 shoulder, out Vector2 elbow, out Vector2 wrist)) yield break;
        Vector2 elbowOffset = elbow - shoulder, wristOffset = wrist - shoulder;
        Vector2 shoulderOffset = shoulder - (Vector2)transform.position;
        float startAngle = side * Mathf.Abs(sweepWindbackAngle);
        float endAngle = -side * Mathf.Abs(sweepFollowThroughAngle);
        float counterPose = -side * 30f;
        float halfTravel = Mathf.Min(1f, Mathf.Max(0f, sweepBodyTravel)) * 0.5f;
        Vector3 start = SweepRootPose(startAngle, side * halfTravel, shoulderOffset, elbowOffset, wristOffset);
        bodyAttackActive = true;
        yield return MoveBody(start, bodyWindupTime, right ? counterPose : startAngle, right ? startAngle : counterPose);
        yield return new WaitForSeconds(Mathf.Max(0f, sweepPoseHold));
        bool hit = false;
        if (!rig.TryGetPose(arm.position, out Vector2 ps, out Vector2 pe, out Vector2 pw))
        { yield return ReturnBody(); bodyAttackActive = false; yield break; }
        float duration = Mathf.Max(0.05f, sweepTravelTime);
        for (float t = 0f; t < duration && CanBodyAttack; t += Time.deltaTime)
        {
            float p = Mathf.Clamp01(t / duration);
            float f = Mathf.SmoothStep(0f, 1f, p);
            float angle = Mathf.Lerp(startAngle, endAngle, f);
            transform.position = SweepRootPose(angle, Mathf.Lerp(side * halfTravel, -side * halfTravel, f), shoulderOffset, elbowOffset, wristOffset);
            SetArmAngles(right ? Mathf.Lerp(counterPose, 0f, f) : angle, right ? angle : Mathf.Lerp(counterPose, 0f, f));
            if (rig.TryGetPose(arm.position, out Vector2 ns, out Vector2 ne, out Vector2 nw))
            {
                if (!hit && SweepHits(player.transform.position, ps, pe, pw, ns, ne, nw))
                { player.TakeDamage(armDamage, arm.gameObject); hit = true; }
                ps = ns; pe = ne; pw = nw;
            }
            yield return null;
        }
        if (CanBodyAttack)
        {
            transform.position = SweepRootPose(endAngle, -side * halfTravel, shoulderOffset, elbowOffset, wristOffset);
            SetArmAngles(right ? 0f : endAngle, right ? endAngle : 0f);
            if (!hit && rig.TryGetPose(arm.position, out Vector2 ns, out Vector2 ne, out Vector2 nw)
                && SweepHits(player.transform.position, ps, pe, pw, ns, ne, nw)) player.TakeDamage(armDamage, arm.gameObject);
        }
        yield return new WaitForSeconds(Mathf.Max(0f, impactHold) + 0.16f);
        // Keep the arm extended during the first lift, then fold it as the body returns.
        Vector3 lifted = transform.position + Vector3.up * Mathf.Max(0f, recoveryLift);
        yield return MoveBody(lifted, armReturnTime * 0.4f, leftAngle, rightAngle);
        yield return MoveBody(bodyHome, armReturnTime * 0.6f);
        bodyAttackActive = false;
    }

    private Vector3 SweepRootPose(float angle, float xOffset, Vector2 shoulder, Vector2 elbow, Vector2 wrist)
    {
        Quaternion q = Quaternion.Euler(0f, 0f, angle);
        Vector2 e = q * (Vector3)elbow, w = q * (Vector3)wrist;
        // Keep the lowest part of the attacking arm near the authored ground-height line.
        float lowest = shoulder.y + Mathf.Min(0f, Mathf.Min(e.y, w.y));
        // Stay low throughout the swing instead of rising and falling with the wrist.
        float neutralLowest = shoulder.y + Mathf.Min(0f, Mathf.Min(elbow.y, wrist.y));
        float lowPose = Mathf.Max(bodyHome.y - Mathf.Max(0f, sweepMaxLowering), sweepWorldY - neutralLowest, sweepWorldY + Mathf.Max(0f, sweepBodyClearance));
        float y = Mathf.Max(lowPose, sweepWorldY - lowest);
        return new Vector3(bodyHome.x + xOffset, y, bodyHome.z);
    }

    private bool SweepHits(Vector2 p, Vector2 ps, Vector2 pe, Vector2 pw, Vector2 ns, Vector2 ne, Vector2 nw)
    {
        return SweptLinkHit(p, ps, pe, ns, ne) || SweptLinkHit(p, pe, pw, ne, nw)
            || DistanceToSegment(p, pw, nw) <= Mathf.Max(0.01f, sweepArmRadius * 1.5f);
    }

    // Swept capsule approximation: both link positions, both moving ends, and the enclosed area.
    private bool SweptLinkHit(Vector2 p, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        float radius = Mathf.Max(0.01f, sweepArmRadius);
        return DistanceToSegment(p, a, b) <= radius || DistanceToSegment(p, c, d) <= radius
            || DistanceToSegment(p, a, c) <= radius || DistanceToSegment(p, b, d) <= radius
            || InTriangle(p, a, b, d) || InTriangle(p, a, d, c);
    }
    private static float Cross(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;
    private static bool InTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        if (Mathf.Abs(Cross(b - a, c - a)) < 0.00001f) return false;
        float x = Cross(b - a, p - a), y = Cross(c - b, p - b), z = Cross(a - c, p - c);
        return (x >= 0f && y >= 0f && z >= 0f) || (x <= 0f && y <= 0f && z <= 0f);
    }

    private IEnumerator Grab(Transform arm)
    {
        if (!arm || !carrier || !CanBodyAttack) yield break;
        bodyAttackActive = true;
        Vector3 target = BodyPositionForHand(arm, player.transform.position);
        yield return MoveBody(target + Vector3.up * Mathf.Max(0.5f, sweepWindupHeight), bodyWindupTime,
            arm == leftArm ? -windupArmAngle : 0f, arm == rightArm ? windupArmAngle : 0f);
        yield return new WaitForSeconds(0.4f);
        yield return MoveBody(target, bodyDropTime, 0f, 0f, true);
        bool grabbed = CanBodyAttack && InRange(arm) && carrier.BeginGrab(arm);
        // Return to the normal hover position with the player in the hand.
        yield return ReturnBody();
        if (grabbed)
        {
            for (int i = 0; i < 3 && CanBodyAttack; i++)
            {
                SpawnHoming();
                yield return new WaitForSeconds(0.65f);
            }
            carrier.EndGrab();
        }
        bodyAttackActive = false;
    }

    private IEnumerator ReturnBody()
    {
        // Lift vertically before drifting back; never drag the hands through the floor.
        Vector3 lifted = transform.position;
        lifted.y = Mathf.Min(bodyHome.y, lifted.y + Mathf.Max(0f, recoveryLift));
        yield return MoveBody(lifted, armReturnTime * 0.4f);
        yield return MoveBody(bodyHome, armReturnTime * 0.6f);
    }

    private IEnumerator MoveBody(Vector3 target, float duration, float endLeft = 0f,
        float endRight = 0f, bool acceleratingStrike = false)
    {
        Vector3 start = transform.position;
        float startLeft = leftAngle, startRight = rightAngle;
        duration = Mathf.Max(0.01f, duration);
        for (float t = 0f; t < duration && CanBodyAttack; t += Time.deltaTime)
        {
            float p = Mathf.Clamp01(t / duration);
            float f = acceleratingStrike ? p * p : Mathf.SmoothStep(0f, 1f, p);
            transform.position = Vector3.Lerp(start, target, f);
            SetArmAngles(Mathf.Lerp(startLeft, endLeft, f), Mathf.Lerp(startRight, endRight, f));
            yield return null;
        }
        if (CanBodyAttack)
        {
            transform.position = target;
            SetArmAngles(endLeft, endRight);
        }
    }

    private void UpdateHover()
    {
        if (bodyAttackActive || State == BossState.Transition || State == BossState.Dead) return;
        float wave = Time.time * Mathf.Max(0f, hoverFrequency) * Mathf.PI * 2f;
        Vector3 home = State == BossState.Phase2 ? phase2Home : bodyHome;
        float y = Mathf.Sin(wave) * hoverHeight;
        if (State == BossState.Stunned) y = -stunnedDrop + y * 0.2f;
        float blend = 1f - Mathf.Exp(-7f * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, home + Vector3.up * y, blend);
        if (State == BossState.Phase1)
            SetArmAngles(Mathf.Lerp(leftAngle, Mathf.Sin(wave + 0.8f) * idleArmAngle, blend),
                         Mathf.Lerp(rightAngle, -Mathf.Sin(wave + 1.5f) * idleArmAngle, blend));
        else if (State != BossState.Phase2) SetArmAngles(Mathf.Lerp(leftAngle, 0f, blend), Mathf.Lerp(rightAngle, 0f, blend));
    }

    private void SetArmAngles(float l, float r)
    {
        leftAngle = l; rightAngle = r;
        RotateHandAroundShoulder(leftArm, leftShoulder, leftHome, l);
        RotateHandAroundShoulder(rightArm, rightShoulder, rightHome, r);
    }

    private static void RotateHandAroundShoulder(Transform hand, Transform shoulder, Vector3 home, float angle)
    {
        if (!hand) return;
        if (!shoulder || !hand.parent) { hand.localPosition = home; return; }
        Vector3 pivot = hand.parent.InverseTransformPoint(shoulder.position);
        Vector3 offset = home - pivot;
        Vector3 rotated = Quaternion.Euler(0f, 0f, angle) * offset;
        hand.localPosition = pivot + rotated;
    }

    private static float DistanceToSegment(Vector2 point, Vector2 a, Vector2 b)
    {
        Vector2 d = b - a;
        float t = d.sqrMagnitude > 0.00001f ? Mathf.Clamp01(Vector2.Dot(point - a, d) / d.sqrMagnitude) : 0f;
        return Vector2.Distance(point, a + d * t);
    }

    private bool InRange(Transform arm) => PlayerAlive && Vector2.Distance(arm.position, player.transform.position) <= armHitRadius;
    private void OnDisable()
    {
        ShowLaserWarning(false);
        HideLaserBeam();
        if (carrier) carrier.EndGrab();
    }
    private void ResetArms() { SetArmAngles(0f, 0f); }
    private static void SetActive(GameObject obj, bool value) { if (obj) obj.SetActive(value); }
}
