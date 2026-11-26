using UnityEngine;
using UnityEngine.Tilemaps;

public class RandomTilemap : MonoBehaviour
{
    // =========================================================
    // Tilemap
    // =========================================================

    [Header("Tilemap")]
    [SerializeField] private Tilemap tilemap;


    // =========================================================
    // Tile Groups
    // =========================================================

    [Header("Ground Tiles")]
    [SerializeField] private TileBase[] groundTiles;

    [Header("Climbable Wall Tiles")]
    [SerializeField] private TileBase[] climbableWallTiles;

    [Header("Background Tiles")]
    [SerializeField] private TileBase[] backgroundTiles;


    // =========================================================
    // Map Size
    // =========================================================

    [Header("Map Size")]
    [SerializeField] private int mapWidth = 100;

    [SerializeField] private int mapHeight = 40;


    // =========================================================
    // Ground
    // =========================================================

    [Header("Ground Generation")]
    [SerializeField] private int minGroundHeight = 4;

    [SerializeField] private int maxGroundHeight = 8;

    [SerializeField] private int maxHeightChange = 2;

    [Range(0f, 1f)]
    [SerializeField] private float heightChangeChance = 0.35f;


    // =========================================================
    // Walls
    // =========================================================

    [Header("Wall Generation")]
    [SerializeField] private int minWallHeight = 3;

    [SerializeField] private int maxWallHeight = 8;

    [Range(0f, 1f)]
    [SerializeField] private float wallChance = 0.15f;


    // =========================================================
    // Floating Platforms
    // =========================================================

    [Header("Floating Platforms")]
    [SerializeField] private int minPlatformCount = 5;

    [SerializeField] private int maxPlatformCount = 12;

    [SerializeField] private int minPlatformLength = 3;

    [SerializeField] private int maxPlatformLength = 7;

    [SerializeField] private int minPlatformHeight = 9;

    [SerializeField] private int maxPlatformHeight = 20;

    [SerializeField] private int minPlatformDistance = 5;

    [SerializeField] private int maxPlatformDistance = 12;


    // =========================================================
    // Background
    // =========================================================

    [Header("Background Generation")]
    [SerializeField] private bool generateBackground = true;

    [SerializeField] private int backgroundBottom = 0;

    [SerializeField] private int backgroundTop = 30;


    // =========================================================
    // Random Seed
    // =========================================================

    [Header("Random Seed")]
    [SerializeField] private bool useRandomSeed = true;

    [SerializeField] private int seed = 12345;


    // =========================================================
    // Player
    // =========================================================

    [Header("Player")]
    [SerializeField] private Transform player;

    [SerializeField] private int playerStartX = 5;


    // =========================================================
    // Internal Data
    // =========================================================

    private int[] groundHeights;


    // =========================================================
    // Start
    // =========================================================

    private void Start()
    {
        GenerateMap();
    }


    // =========================================================
    // Update
    // =========================================================

    private void Update()
    {
        // 테스트용
        // R 키를 누르면 새로운 맵 생성

        if (Input.GetKeyDown(KeyCode.R))
        {
            GenerateMap();
        }
    }


    // =========================================================
    // Generate Map
    // =========================================================

    public void GenerateMap()
    {
        if (tilemap == null)
        {
            Debug.LogError(
                "RandomTilemap : Tilemap이 연결되지 않았습니다."
            );

            return;
        }


        if (groundTiles == null ||
            groundTiles.Length == 0)
        {
            Debug.LogError(
                "RandomTilemap : Ground Tiles가 없습니다."
            );

            return;
        }


        // -----------------------------------------------------
        // Seed
        // -----------------------------------------------------

        if (useRandomSeed)
        {
            seed = Random.Range(
                0,
                999999999
            );
        }

        Random.InitState(seed);


        Debug.Log(
            "================================"
        );

        Debug.Log(
            "Random Map 생성"
        );

        Debug.Log(
            "Seed : " + seed
        );

        Debug.Log(
            "================================"
        );


        // -----------------------------------------------------
        // 기존 타일 삭제
        // -----------------------------------------------------

        tilemap.ClearAllTiles();


        // -----------------------------------------------------
        // Ground Height 배열
        // -----------------------------------------------------

        groundHeights =
            new int[mapWidth];


        // -----------------------------------------------------
        // 1. Background
        // -----------------------------------------------------

        GenerateBackground();


        // -----------------------------------------------------
        // 2. Ground
        // -----------------------------------------------------

        GenerateGround();


        // -----------------------------------------------------
        // 3. Walls
        // -----------------------------------------------------

        GenerateWalls();


        // -----------------------------------------------------
        // 4. Floating Platforms
        // -----------------------------------------------------

        GenerateFloatingPlatforms();


        // -----------------------------------------------------
        // 5. Player
        // -----------------------------------------------------

        SetPlayerStartPosition();


        Debug.Log(
            "Random Map 생성 완료"
        );
    }


    // =========================================================
    // Get Random Ground Tile
    // =========================================================

    private TileBase GetRandomGroundTile()
    {
        if (groundTiles == null ||
            groundTiles.Length == 0)
        {
            return null;
        }


        return groundTiles[
            Random.Range(
                0,
                groundTiles.Length
            )
        ];
    }


    // =========================================================
    // Get Random Wall Tile
    // =========================================================

    private TileBase GetRandomWallTile()
    {
        if (climbableWallTiles == null ||
            climbableWallTiles.Length == 0)
        {
            // 벽 타일을 지정하지 않았다면
            // Ground Tile을 대신 사용

            return GetRandomGroundTile();
        }


        return climbableWallTiles[
            Random.Range(
                0,
                climbableWallTiles.Length
            )
        ];
    }


    // =========================================================
    // Get Random Background Tile
    // =========================================================

    private TileBase GetRandomBackgroundTile()
    {
        if (backgroundTiles == null ||
            backgroundTiles.Length == 0)
        {
            return null;
        }


        return backgroundTiles[
            Random.Range(
                0,
                backgroundTiles.Length
            )
        ];
    }


    // =========================================================
    // Background
    // =========================================================

    private void GenerateBackground()
    {
        if (!generateBackground)
            return;


        if (backgroundTiles == null ||
            backgroundTiles.Length == 0)
        {
            return;
        }


        for (int x = 0; x < mapWidth; x++)
        {
            for (
                int y = backgroundBottom;
                y <= backgroundTop;
                y++
            )
            {
                TileBase tile =
                    GetRandomBackgroundTile();


                if (tile == null)
                    continue;


                tilemap.SetTile(
                    new Vector3Int(
                        x,
                        y,
                        0
                    ),
                    tile
                );
            }
        }
    }


    // =========================================================
    // Ground
    // =========================================================

    private void GenerateGround()
    {
        int currentHeight =
            Random.Range(
                minGroundHeight,
                maxGroundHeight + 1
            );


        for (int x = 0; x < mapWidth; x++)
        {
            // -------------------------------------------------
            // 높이 변경
            // -------------------------------------------------

            if (Random.value < heightChangeChance)
            {
                int heightChange =
                    Random.Range(
                        -maxHeightChange,
                        maxHeightChange + 1
                    );


                currentHeight +=
                    heightChange;
            }


            // -------------------------------------------------
            // 높이 제한
            // -------------------------------------------------

            currentHeight =
                Mathf.Clamp(
                    currentHeight,
                    minGroundHeight,
                    maxGroundHeight
                );


            groundHeights[x] =
                currentHeight;


            // -------------------------------------------------
            // Ground 생성
            // -------------------------------------------------

            for (int y = 0; y <= currentHeight; y++)
            {
                TileBase tile =
                    GetRandomGroundTile();


                tilemap.SetTile(
                    new Vector3Int(
                        x,
                        y,
                        0
                    ),
                    tile
                );
            }
        }
    }


    // =========================================================
    // Walls
    // =========================================================

    private void GenerateWalls()
    {
        for (int x = 1; x < mapWidth - 1; x++)
        {
            int previousHeight =
                groundHeights[x - 1];

            int currentHeight =
                groundHeights[x];


            // -------------------------------------------------
            // 지형 높이 차이
            // -------------------------------------------------

            int heightDifference =
                Mathf.Abs(
                    currentHeight -
                    previousHeight
                );


            // -------------------------------------------------
            // 높이 차이가 크면 벽 생성
            // -------------------------------------------------

            if (heightDifference >= 2)
            {
                int lowerHeight =
                    Mathf.Min(
                        previousHeight,
                        currentHeight
                    );


                int higherHeight =
                    Mathf.Max(
                        previousHeight,
                        currentHeight
                    );


                for (
                    int y = lowerHeight;
                    y <= higherHeight;
                    y++
                )
                {
                    TileBase wall =
                        GetRandomWallTile();


                    tilemap.SetTile(
                        new Vector3Int(
                            x,
                            y,
                            0
                        ),
                        wall
                    );
                }
            }


            // -------------------------------------------------
            // 랜덤 벽
            // -------------------------------------------------

            if (Random.value < wallChance)
            {
                int wallHeight =
                    Random.Range(
                        minWallHeight,
                        maxWallHeight + 1
                    );


                int baseHeight =
                    groundHeights[x];


                for (
                    int y = 1;
                    y <= wallHeight;
                    y++
                )
                {
                    int tileY =
                        baseHeight + y;


                    if (tileY >= mapHeight)
                        break;


                    TileBase wall =
                        GetRandomWallTile();


                    tilemap.SetTile(
                        new Vector3Int(
                            x,
                            tileY,
                            0
                        ),
                        wall
                    );
                }
            }
        }
    }


    // =========================================================
    // Floating Platforms
    // =========================================================

    private void GenerateFloatingPlatforms()
    {
        int platformCount =
            Random.Range(
                minPlatformCount,
                maxPlatformCount + 1
            );


        int lastPlatformX = 0;


        for (
            int i = 0;
            i < platformCount;
            i++
        )
        {
            // -------------------------------------------------
            // 플랫폼 X
            // -------------------------------------------------

            int minX =
                lastPlatformX +
                minPlatformDistance;


            int maxX =
                Mathf.Min(
                    mapWidth -
                    maxPlatformLength -
                    1,

                    lastPlatformX +
                    maxPlatformDistance
                );


            if (minX > maxX)
                break;


            int startX =
                Random.Range(
                    minX,
                    maxX + 1
                );


            // -------------------------------------------------
            // 플랫폼 길이
            // -------------------------------------------------

            int length =
                Random.Range(
                    minPlatformLength,
                    maxPlatformLength + 1
                );


            // -------------------------------------------------
            // 플랫폼 높이
            // -------------------------------------------------

            int height =
                Random.Range(
                    minPlatformHeight,
                    maxPlatformHeight + 1
                );


            // -------------------------------------------------
            // 플랫폼 생성
            // -------------------------------------------------

            CreatePlatform(
                startX,
                height,
                length
            );


            lastPlatformX =
                startX + length;
        }
    }


    // =========================================================
    // Create Platform
    // =========================================================

    private void CreatePlatform(
        int startX,
        int height,
        int length
    )
    {
        for (
            int x = 0;
            x < length;
            x++
        )
        {
            int tileX =
                startX + x;


            if (tileX < 0 ||
                tileX >= mapWidth)
            {
                continue;
            }


            if (height < 0 ||
                height >= mapHeight)
            {
                continue;
            }


            TileBase tile =
                GetRandomGroundTile();


            tilemap.SetTile(
                new Vector3Int(
                    tileX,
                    height,
                    0
                ),
                tile
            );
        }


        // -----------------------------------------------------
        // 플랫폼 끝에 벽 추가
        // -----------------------------------------------------

        CreatePlatformWall(
            startX,
            height
        );


        CreatePlatformWall(
            startX + length - 1,
            height
        );
    }


    // =========================================================
    // Platform Wall
    // =========================================================

    private void CreatePlatformWall(
        int x,
        int height
    )
    {
        int wallHeight =
            Random.Range(
                2,
                5
            );


        for (
            int y = 1;
            y <= wallHeight;
            y++
        )
        {
            int tileY =
                height + y;


            if (tileY >= mapHeight)
                break;


            TileBase tile =
                GetRandomWallTile();


            tilemap.SetTile(
                new Vector3Int(
                    x,
                    tileY,
                    0
                ),
                tile
            );
        }
    }


    // =========================================================
    // Player Start Position
    // =========================================================

    private void SetPlayerStartPosition()
    {
        if (player == null)
            return;


        int x =
            Mathf.Clamp(
                playerStartX,
                0,
                mapWidth - 1
            );


        int groundY =
            groundHeights[x];


        player.position =
            new Vector3(
                x + 0.5f,
                groundY + 1.5f,
                player.position.z
            );
    }


    // =========================================================
    // Regenerate
    // =========================================================

    public void Regenerate()
    {
        GenerateMap();
    }


    // =========================================================
    // Get Ground Height
    // =========================================================

    public int GetGroundHeight(int x)
    {
        if (groundHeights == null)
            return minGroundHeight;


        if (x < 0 ||
            x >= groundHeights.Length)
        {
            return minGroundHeight;
        }


        return groundHeights[x];
    }


    // =========================================================
    // Gizmos
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        if (groundHeights == null)
            return;


        Gizmos.color =
            Color.green;


        for (
            int x = 0;
            x < groundHeights.Length - 1;
            x++
        )
        {
            Vector3 start =
                new Vector3(
                    x + 0.5f,
                    groundHeights[x] + 0.5f,
                    0f
                );


            Vector3 end =
                new Vector3(
                    x + 1.5f,
                    groundHeights[x + 1] + 0.5f,
                    0f
                );


            Gizmos.DrawLine(
                start,
                end
            );
        }
    }
}