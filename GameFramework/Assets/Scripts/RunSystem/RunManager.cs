using UnityEngine;
using UnityEngine.SceneManagement;

public enum RunPhase
{
    Hub,
    Combat,
    Shop,
    Boss,
    Clear
}

public class RunManager : MonoBehaviour
{
    public static RunManager Instance { get; private set; }

    [Header("Scene Settings")]
    [SerializeField] private string hubSceneName = "Hub";
    [SerializeField] private string runClearSceneName = "";

    [Header("Run Themes")]
    [SerializeField] private RunThemeData[] themes;

    [Header("Run Stats")]
    [SerializeField] private RunStatModifiers runStats = new();

    // 현재 진행 정보
    public int CurrentThemeIndex { get; private set; } = -1;
    public int CurrentStageNumber { get; private set; } = 1;

    public RunPhase CurrentPhase { get; private set; } = RunPhase.Hub;

    public bool IsRunActive { get; private set; }

    public RunStatModifiers RunStats => runStats;

    public RunThemeData CurrentTheme
    {
        get
        {
            if (CurrentThemeIndex < 0 ||
                CurrentThemeIndex >= themes.Length)
                return null;

            return themes[CurrentThemeIndex];
        }
    }

    private bool isTransitioning;

    // =========================================
    // Singleton
    // =========================================

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        if (Instance == this)
            SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // =========================================
    // Start Run
    // =========================================

    public void StartRun()
    {
        if (IsRunActive || isTransitioning)
            return;

        if (themes == null || themes.Length == 0)
        {
            Debug.LogError("등록된 런 테마가 없습니다.");
            return;
        }

        runStats.Reset();

        IsRunActive = true;

        CurrentThemeIndex = 0;
        CurrentStageNumber = 2;

        CurrentPhase = RunPhase.Combat;

        Debug.Log("새로운 RUN 시작!");

        LoadRandomCombatStage();
    }

    // =========================================
    // Combat Stage
    // =========================================

    // 일반 전투 스테이지 클리어
    public void OnCombatStageCleared()
    {
        if (!CanProgress(RunPhase.Combat))
            return;

        Debug.Log(
            $"{CurrentTheme.ThemeName} Stage {CurrentStageNumber} 클리어!"
        );

        if (CurrentStageNumber < 4)
        {
            CurrentStageNumber++;

            LoadRandomCombatStage();
        }
        else
        {
            // Stage 4 클리어 -> Stage 5 상점
            CurrentStageNumber = 5;

            CurrentPhase = RunPhase.Shop;

            LoadScene(CurrentTheme.ShopSceneName);
        }
    }

    // 랜덤 전투 씬 선택
    private void LoadRandomCombatStage()
    {
        StageData selectedData =
            CurrentTheme.GetRandomStageData(CurrentStageNumber);

        if (selectedData == null)
            return;

        Debug.Log(
            $"현재 테마: {CurrentTheme.ThemeName} / " +
            $"Stage: {CurrentStageNumber} / " +
            $"선택된 씬: {selectedData.SceneName}"
        );

        LoadScene(selectedData.SceneName);
    }

    // =========================================
    // Shop
    // =========================================

    // 상점 구매 및 준비 완료
    public void OnShopFinished()
    {
        if (!CanProgress(RunPhase.Shop))
            return;

        CurrentPhase = RunPhase.Boss;

        Debug.Log(
            $"{CurrentTheme.ThemeName} 보스방 입장!"
        );

        LoadScene(CurrentTheme.BossSceneName);
    }

    // =========================================
    // Boss
    // =========================================

    // 보스 처치
    public void OnBossDefeated()
    {
        if (!CanProgress(RunPhase.Boss))
            return;

        Debug.Log(
            $"{CurrentTheme.ThemeName} 보스 처치!"
        );

        CurrentThemeIndex++;

        // 모든 테마 클리어 여부
        if (CurrentThemeIndex >= themes.Length)
        {
            OnRunCleared();
            return;
        }

        // 다음 테마 시작
        CurrentStageNumber = 2;

        CurrentPhase = RunPhase.Combat;

        Debug.Log(
            $"다음 테마 시작: {CurrentTheme.ThemeName}"
        );

        LoadRandomCombatStage();
    }

    // =========================================
    // Run Clear
    // =========================================

    private void OnRunCleared()
    {
        IsRunActive = false;

        CurrentPhase = RunPhase.Clear;

        Debug.Log("모든 테마 클리어! RUN CLEAR!");

        string destination = string.IsNullOrWhiteSpace(runClearSceneName)
            ? hubSceneName
            : runClearSceneName;

        LoadScene(destination);
    }

    // =========================================
    // Player Death
    // =========================================

    // 사망 애니메이션이 끝난 뒤 호출할 함수
    public void OnPlayerDeath()
    {
        if (!IsRunActive || isTransitioning)
            return;

        Debug.Log("RUN 실패!");

        runStats.Reset();

        IsRunActive = false;

        CurrentThemeIndex = -1;
        CurrentStageNumber = 1;

        CurrentPhase = RunPhase.Hub;

        LoadScene(hubSceneName);
    }

    // =========================================
    // Scene Management
    // =========================================

    private void LoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("이동할 씬 이름이 비어 있습니다.");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError(
                $"씬을 불러올 수 없습니다: {sceneName}\n" +
                "Build Profiles의 Scene List에 등록되어 있는지 확인하세요."
            );

            return;
        }

        isTransitioning = true;

        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    private bool CanProgress(RunPhase expectedPhase)
    {
        return IsRunActive &&
               !isTransitioning &&
               CurrentPhase == expectedPhase &&
               CurrentTheme != null;
    }

    // =========================================
    // Scene Loaded
    // =========================================

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isTransitioning = false;

        Debug.Log($"씬 이동 완료: {scene.name}");
    }
}