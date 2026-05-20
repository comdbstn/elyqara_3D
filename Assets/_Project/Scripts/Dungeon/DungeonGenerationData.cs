using UnityEngine;

namespace Elyqara.Dungeon
{
    // 단계 13 — 자동 생성 미로 데이터. 1층 (Stage1+2+3) 공통 (사용자 결정 = "층마다 테마 같음").
    // CLAUDE.md 원칙 4 — 추가 = 데이터 추가. 런타임 RNG 입력값.
    [CreateAssetMenu(fileName = "DungeonGenerationData", menuName = "Elyqara/Dungeon Generation Data", order = 110)]
    public sealed class DungeonGenerationData : ScriptableObject
    {
        [Header("Grid")]
        [Min(10)] public int gridWidth = 50;
        [Min(10)] public int gridHeight = 50;
        [Min(1)] public float cellSize = 4f;  // 1 cell = 4 unit (캡슐 Player 정합)

        [Header("Room Placement")]
        [Min(2)] public int roomCount = 15;
        [Min(2)] public int minRoomSize = 3;
        [Min(2)] public int maxRoomSize = 14;       // 작은 방 ~ 큰 홀 다양성
        [Min(1)] public int roomBuffer = 2;        // 방 간 버퍼 cell
        [Min(10)] public int maxPlacementAttempts = 400;

        [Header("Corridors")]
        [Range(0f, 1f)] public float extraCorridorRatio = 0.4f;  // 트리 외 여분 통로 비율 — loop(갈림길) 생성

        [Header("Props")]
        public GameObject[] propPrefabs;            // 방에 배치할 prop (궤짝 등). 시드 결정론 — 멀티 동기화 자동
        [Range(0f, 1f)] public float propRoomChance = 0.5f;  // Generic 방에 prop 놓을 확률

        [Header("Walls")]
        [Min(1)] public float wallHeight = 4f;
        [Min(0.05f)] public float wallThickness = 0.3f;

        [Header("Materials")]
        public Material floorMaterial;
        public Material wallMaterial;

        [Header("Lighting (Souls-like)")]
        public bool placePointLights = true;
        public Color lightColor = new Color(1f, 0.6f, 0.3f);
        [Min(1)] public float lightRange = 8f;
        [Min(0)] public float lightIntensity = 1.5f;
    }
}
