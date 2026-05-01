# Elyqara_3D — 차기작 프로젝트

> 2026-05-01 비전 풀 락. 코딩 전 반드시 숙지. 옛 비전(4-30)은 `CLAUDE.md.legacy-20260430` 참조.
> **상세 참조** (필요 시 클로드가 Read):
> - 패키지 추가 SOP / 환경 fix / Unity MCP 워크플로우 / 알려진 함정 → `docs/sop.md`
> - manifest 패키지 표 + 시스템 도구 상태 → `docs/packages.md`
> - 결정 로그 (단계별 사용자 직인 + 함정 학습) → `docs/decisions.md`
> - Elyqara 2D read-only 레퍼런스 + 커리어 컨텍스트 → `docs/elyqara-2d-ref.md`

---

## 한 줄 정의

> **친구 3명이랑 좁은 던전 골목에서, 적 한 마리 한 마리 무겁게 잡으면서, 같이 본 드랍을 분배해 그날의 빌드를 만들어가는 3D 코옵 액션 로그라이크.**

## 머릿속 핵심 그림

> "탱커가 앞에서 방패를 들고 뒤에서 딜러와 힐러가 좁은 던전 골목에서 전투하는 그림"

---

## 🚨 절대 원칙 (Elyqara 폭발 재발 방지)

**진단 (사용자 직인 2026-05-01)**: *"elyqara가 ai환상이랑 메모리인가? ... 너가 짠 코드들 사이에서 상호작용을 못했어 특히 예전에 짰던 코드들"*

**핵심 원흉**: 클로드가 새 기능 추가 시 옛 코드 안 보고 옆에 새 시스템 만든 것. 같은 카테고리가 여러 패턴으로 흩어져 코드 카오스 → 사용자 갈피 잃음 → 중단.

### 10 원칙

1. **★ 옛 코드 우선 확인** — 새 기능 추가 전 같은 카테고리 기존 코드 grep/Read 강제. "이미 있는 패턴이 뭐지?" 자문하고 못 찾으면 사용자에게 묻기. 옛 코드 무시 금지
2. **단일 매니저** — 카테고리당 매니저 1개 (적/스킬/아이템/UI/방 등). 옆에 새 매니저 만들지 말 것
3. **인터페이스 우선** — 모든 적은 IEnemy, 모든 스킬은 ISkill, 모든 아이템은 IItem. 새 코드가 인터페이스 없이 추가되지 못하게 구조
4. **추가 = 데이터 추가** — 새 캐릭/적/아이템 = ScriptableObject 한 장 + 코드 0~1줄. 코드 추가 필요하면 인터페이스 부족 신호
5. **★ 환상 금지** — 코드 추가 전 기존 패턴/파일/타입 실제 확인. 추측해서 만들지 말 것
    - **★ 값/버전/ID/임계값 결정도 동일 룰**. 작성 직전 자문: *"이 값이 어디서 왔나? 메모리/방금 읽은 docs/방금 읽은 코드 에서 직접 인용 가능한가?"*
    - **Citation-first** — 못 가르치면 추측 X. 사용자에게 묻기 / WebSearch 한 번 더 / 즉시 *"확인 안 됨"* 표시 후 멈춤
    - **위험 신호 = 환상의 가면**: *"안전한 보수"* / *"likely correct"* / *"보통 그렇다"* / *"보수적 stable"* — 이런 표현 떠오르면 **그 자체가 추측 신호**. 멈추기
    - **Mid-response retraction** — 작성 도중 의심 들면 즉시 멈추고 사용자에게 보고. 끝까지 작성 후 사용자 확인 X
    - **불확실성 인정 우선** — *"모르겠다"* 가 추측 답 보다 항상 우선. 사용자 직인 2026-05-02 *"가장중요해 저건 추측안하기"*
    - **트리거 사례** (재발 방지): 2026-05-02 ProBuilder 6.0.4 — Claude 가 *"안전한 보수 6.0.4"* 추측. WebSearch 결과에 6.0.4 source 0건이었는데도 추가 → Editor fatal crash. 정공 = 사용자에게 Package Manager UI 통한 설치 부탁
6. **메모리 즉시 갱신** — 사용자 결정/명확화는 그 턴에 적용. 글로벌 메모리(`~/.claude/projects/-Users-jeong-yunsu-Elyqara-3D/memory/`)와 이 CLAUDE.md (또는 docs/) 둘 다
7. **컨벤션 일관성** — 한 번 정한 패턴은 모든 비슷한 케이스에서 동일하게. "예외" 만들지 말 것
8. **YAGNI** — 지금 안 쓰는 추상화/헬퍼/유틸리티 만들지 말 것. 3번 같으면 그때 추출
9. **단일 책임** — 한 파일/클래스가 한 가지만. Elyqara 에서 폭발한 신 클래스(StageManager 등) 패턴 반복 금지
10. **★ 정공법 우선 — 빠른 회피 옵션 제시 금지** (사용자 직인 2026-05-01: *"무조건 정공법으로 가 빠른건 그냥 배재해 장기프로젝트야 미래에 어떤 문제도 되지않게 정공법을 기본으로 박아"*)
    - 마찰/문제 만나면 **근본 원인 처리 + 재발 방지 hook 까지가 한 단위.** "임시 회피" / "일단 빠르게" — 모두 장기 부채. 금지
    - "방법 A 빠르게 / 방법 B 클린하게" 두 옵션 제시 X — **클린 옵션만 제시.** 사용자가 시간/체력 부족하면 *작업 중단* 이 답. *회피* X
    - 환경 문제도 동일 — "Editor 재시작이 정공" 같은 안내가 회피 아닌 정공이면 그것만 제시. 가능하면 코드 레벨 hook 으로 자동화

### 새 기능 추가 SOP

1. 사용자 요구 받음
2. **외부 자료 검색** — 표준 패턴 (3인칭 카메라 / AI / 인벤 / 네트워킹) 은 WebSearch 또는 Explore 로 한 번 검증된 코드 검색. 키워드: "Unity 6000.3 + [기능] + Netcode for GameObjects" / "Cinemachine 3.x + [기능]". **결과 명시 보고** (사용자 SOP 위반 인지 방지)
3. **관련 카테고리 grep** — "비슷한 거 이미 있나?"
4. **인터페이스/매니저 확인** — "이 카테고리는 어떻게 굴러가지?"
5. 옛 패턴 따라서 추가. 안 맞으면 "왜 안 맞는지" 보고하고 사용자 확인
6. 데이터로 풀 수 있으면 코드 추가 0줄로 끝
7. 메모리 갱신 + 시스템 인덱스 한 줄 추가

> 패키지 추가 / 환경 fix / Unity MCP 워크플로우 상세 → `docs/sop.md`

---

## 비전 핵심 9개 (절대 흔들지 않음)

1. **전투 톤**: 다크소울/MH식 무게감. 한 마리 한 마리 집중. 뱀서식·RoR2식 잡몹 학살 X
2. **던전**: 던전본/바로니식 전통 미궁. 좁은 통로/방. RoR2식 야외 X
3. **협동**: 역할 강제 X (자율적 역할구조). 같은 드랍을 같이 보고 분배 = 협동의 본질
4. **무기**: 캐릭터별 고정 (코만도 풍). 첫 캐릭 = 검방패 (검=참격, 방패=타격)
5. **빌드**: BG3/바간테식 RPG 아이템 랜덤 드랍. 무기+방어구. Elyqara 디아블로식 그리드 차용
6. **속성 1차**: 참격 + 타격만. 장기 B(2 레이어). 원소·관통 등은 백로그
7. **효과 1차**: 속성당 데미지 % 증가만. 회복/시너지/메커니즘 변화는 백로그
8. **인원·런**: 1~4명 호스트 권한. 30~45분. 다운→부활. 전원 다운=게임오버
9. **스코프 정책**: 기능 메모 ≠ 첫 마일스톤. 1차 단순화, 백로그 풍부

---

## 공격 속성 시스템 (★ 확장의 기준 데이터 구조)

**사용자 직인 2026-05-01**: *"캐릭터 관련 세부사항은 지금 필요없어 그냥 공격 속성들을 여러개 변수같은거로 만들어둬서 캐릭터 확장성만 높이면돼"*

→ 공격 속성을 **변수/데이터**로 추출. 새 캐릭/무기/스킬 = "어떤 속성 가지는지" 데이터로만 명시. 코드 추가 X.

```csharp
public enum PhysicalType { Slash, Blunt, Pierce }  // 1차 = Slash/Blunt 만. Pierce 백로그
public enum ElementType { None, Fire, Cold, Lightning }  // 1차 = None 만. 원소 백로그

[System.Serializable]
public class AttackProperty {
    public PhysicalType physical;
    public ElementType element = ElementType.None;
    public float damageMultiplier = 1f;
}
```

- 검방패 캐릭 = 검(Slash) + 방패(Blunt) 두 무브셋
- 새 캐릭 추가 = 무기 데이터 + 속성 조합 정의로 끝. 코드 추가 0줄 지향
- 효과 풀 1차 = "Slash 데미지 +X%" / "Blunt 데미지 +X%". 회복/시너지/메커니즘 변화는 백로그

---

## 마일스톤 (시간 X, 명제 단위)

> **검증 목표: 검방패 4명이 좁은 던전에서 적 한 마리 무겁게 잡고, 드랍 분배해서 빌드 형성하는 한 런이 재밌는가**

| # | 단계 | 출력 검증 | 상태 |
|---|---|---|---|
| 1 | NGO + Steamworks, Capsule 2명 동기화 | "둘이 같이 움직임" | ✅ |
| 2 | 검방패 캐릭터 슬롯/자원 그릇 | "캐릭터 슬롯/자원 그릇 완성" | ✅ |
| 3 | 더미 적 1종 + 한 마리 집중 AI | "혼자 적 한 마리 잡는 게 재밌음" | ✅ |
| 4 | 2~4명 멀티 동기화 + 적 AI Souls-like 표준 | "같이 잡는 게 더 재밌음" | ✅ |
| 5 | 좁은 던전 1방 (ProBuilder 그레이박스) | "좁은 공간 포지셔닝 의미 있음" | ✅ |
| 6 | 던전 확장 (직접 맵핑 7-room + Door socket 정렬) | "한 런 길이 감" | ✅ |
| 7 | 빌드 시스템 (Elyqara 그리드 인벤토리 차용 + F키 픽업) | "적 잡으면 뭔가 떨어짐" | ✅ |
| 8 | 자유 픽업 드랍 — 4명 검증 (코드 X, 단계 11 합쳐질 가능성) | "협동의 본질이 굴러감" | |
| 9 | 다운/부활 + 데미지 파이프라인 | "한 명 죽어도 끝 아님" | 🟢 다음 |
| 10 | 보스 + 한 런 사이클 | "한 런이 완전 굴러감" | |
| **11** | **🎯 코어 검증 플레이테스트 (친구 4명)** | "재밌는가? 다시 하고 싶은가?" | |

### 코어 검증 통과 후 (큰 그림만)
- M12 콘텐츠 확장 (적/아이템/던전 다양성) / M13 외형 정체성 / M14 폴리싱 / M15 Steam 페이지 + 출시

**기간 중요 X — 명제 만족 시점이 검증.** 호텔 일 + N1 공부 + 게임 사이드 강등 정합 (사용자 직인: *"주차별말고 그냥 순서로 해줘"*).

### 첫 마일스톤 백로그 (포함 X)
- 마스터리 / 메타 통화 / 영구 진행
- 회복 % / 시너지 발동 / 메커니즘 변화 효과
- 원소 속성 (Fire/Cold/Lightning) / Pierce
- 두 번째 캐릭 (Tier 1 코만도풍 1캐릭 4명)
- 허브 / 마스터리 화면 / 캐릭터 선택
- 외형 정체성 (모델/애니/이름/VFX)
- 도발 스킬 / 정밀 어그로 시스템

---

## 기술 스택 (요약)

- **엔진**: Unity 6000.3.7f1
- **네트워킹**: Unity Netcode for GameObjects 2.4.4 (NGO 1.x deprecated)
- **트랜스포트**: 1차 = NGO UTP. Steam P2P (Facepunch.Steamworks) = 단계 1 후 별도 솔루션 (커뮤니티 버전 컴파일 깨짐)
- **권한 모델**: 호스트 권한 (Host-authoritative)
- **카메라**: Cinemachine 3.1.6 (`CinemachineCamera`. CM2 API 사용 X)
- **레벨 모델링**: ProBuilder 6.0.9 (Package Manager UI 통한 설치만)
- **렌더링**: URP 17.3.0 / Input: New Input System 1.18.0 / NavMesh: AI Navigation 2.0.9
- **MPPM**: 멀티 동기화 검증 도구 (Multiplayer Play Mode 2.0.2)

> 패키지 표 / 시스템 도구 상태 / Steam P2P 보류 결정 → `docs/packages.md`

### 멀티 우선 아키텍처 (1주차부터)
1. 모든 게임 상태는 호스트 권위
2. 절차생성 = 시드 동기화 (단계 6 = 직접 맵핑 채택, 절차생성 X)
3. 싱글플레이 = "혼자 호스트"하는 케이스. 오프라인 모드 따로 X

---

## 시스템 인덱스 — 현재 상태 (2026-05-02)

- **단계 1~6 ✅** 통과 (상세 결정 + 사용자 직인 → `docs/decisions.md`)
- **현재 상태**: 단계 6 통과 — 7-room 던전 mesh 정렬 + Editor 도구 3장. 단계 7 (빌드 시스템) 진입 준비
- **코드 폴더**: `Assets/_Project/Scripts/{Networking,Player,Characters,Enemies,Skills,Dungeon}/` (asmdef 6개 분리) + `Assets/_Project/Editor/{Phase6Setup,RoomSnapTool,DungeonGenerator}.cs`
- **씬**: `SampleScene` 에 `[Network]` (NetworkBootstrap) + `[DungeonManager]` + StartRoom + Room_N/E/S/W/E2/E3 + Main Camera (CinemachineBrain)
- **데이터**: `Assets/_Project/Data/Characters/Kiyan.asset` + `Assets/_Project/Data/Rooms/{StartRoom,Room_N,...}Data.asset`
- **GitHub**: `https://github.com/comdbstn/elyqara_3D` main 브랜치
- **Steam AppID**: 480 (Spacewar) — dev 테스트용. 실제 출시 전 교체 필수
- **MCP**: `unity-mcp` Connected. ★ 함정 = 같은 session 안 Editor 재시작 시 schema 캐싱 → 새 session 까지 우회

### 카테고리별 인덱스 (인터페이스 + 매니저 + 데이터 패턴)

- **네트워킹** ✅: `Elyqara.Networking` asmdef. `NetworkBootstrap` (Host/Client/Server OnGUI + UnityTransport 자동 link + PlayMode Stop 시 강제 Shutdown hook). 진짜 인프라(트랜스포트/부트스트랩)만
- **플레이어** ✅: `Elyqara.Player` asmdef. `PlayerInput` (Move/Look + 4슬롯, Owner-only enable) / `PlayerMovement : NetworkBehaviour` (server-auth Rigidbody.linearVelocity) / `PlayerResources : NetworkBehaviour` (NetworkVariable HP/Stamina + regen) / `PlayerCamera : NetworkBehaviour` (vCam SetParent(null) + LateUpdate 직접 추적) / `PlayerCharacterBinder` (CharacterData ref) / `PlayerSkillExecutor` (4슬롯 → ISkill)
- **캐릭터** ✅: `Elyqara.Characters` asmdef. `CharacterData : ScriptableObject` (HP/Stamina + 4 슬롯). 첫 캐릭 = `Kiyan.asset`. 새 캐릭 = SO 한 장 + Player.prefab 의 PlayerCharacterBinder.character 한 줄 변경
- **적** ✅: `Elyqara.Enemies` asmdef. `IEnemy` + `EnemyData : ScriptableObject` (HP/속도/어그로/4-phase attack/stopping distance/콘 hitbox) + `EnemyController : NetworkBehaviour` **Souls-like 6-state FSM** (Idle/Chase/Anticipation/Active/Recovery/Dead) + Transition() 단일 진입점 + 4-phase attack + Stopping Distance + 콘 OverlapSphere hitbox. 첫 적 = `Wisp.asset`. `EnemySpawner` 호스트 시작 시 1마리 (Phase 5+ StartRoom prefab 자식)
- **스킬** ✅: `Elyqara.Skills` asmdef. `ISkill` + `SkillData : ScriptableObject, ISkill` (추상) + `BasicMeleeSkill` (구체) + `IDamageable` (Player/Enemy 둘 다 구현). 첫 스킬 = `BasicMelee.asset`
- **던전** ✅: `Elyqara.Dungeon` asmdef. `IRoom` + `DoorDirection` enum (N/E/S/W) + `RoomData : SO` (isStartRoom flag) + `Room : MonoBehaviour, IRoom` (4 door socket + spawnPoints[] + OnEnable self-register + Editor Gizmos N/E/S/W 색깔 라벨) + `DungeonManager` (Singleton) + `PlayerSpawnPositioner : NetworkBehaviour` (server-side OnNetworkSpawn 시 transform.position 설정 — NGO 2.x ConnectionApproval 버그 회피). **방 = StartRoom + 6개 복제** mesh 끼리 직접 정렬 (절차생성/통로 X). Editor 도구 = `Phase6Setup` (one-shot setup) + `RoomSnapTool` (Cmd+Alt+S) + `DungeonGenerator` (7-room)
- **아이템** 🟢 단계 7: IItem + InventoryManager + ItemData SO + 적 죽음 → ItemDrop 파이프라인. Elyqara 2D read-only 참조 → `docs/elyqara-2d-ref.md`
- **공격 속성**: AttackProperty 데이터 구조 (위 정의)

---

## 절대 사수 체크리스트 (검토 시)

- [ ] 모든 효과/스킬이 *한 줄 설명* 가능?
- [ ] 자율적 역할구조 깨졌나? (캐릭터가 역할 강제하지 않는지)
- [ ] M13(외형) 전에 캐릭터 정체성에 시간 안 썼나?
- [ ] 단계 11 코어 검증 통과했나?
- [ ] 호스트 권한 모델 깨진 곳 없나?
- [ ] 강제 협력 / FF / 정밀 어그로 들어가지 않았나?
- [ ] 88퍽 같은 시스템 폭증 패턴 재발 안 했나?
- [ ] 옛 코드 안 보고 옆에 새 시스템 만든 곳 없나? (★)
- [ ] 새 기능이 데이터 추가로 끝나는가? (★)
- [ ] 단일 매니저 / 인터페이스 우선 지켜졌나? (★)
- [ ] 메모리 즉시 갱신 했나? (★)
- [ ] 빠른 회피 옵션 제시했나? (★) — 클린 옵션만. "임시" 우회 코드/설정 없는지 확인
- [ ] 마찰 만난 부분에 재발 방지 hook 추가됐나? (★)
- [ ] 표준 패턴 진입 전 외부 자료 (인터넷/깃) 검색했나? **결과 명시 보고** (★)
