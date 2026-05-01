# Elyqara_3D — 차기작 프로젝트

> 2026-05-01 비전 풀 락. 코딩 전 반드시 숙지. 옛 비전(4-30)은 `CLAUDE.md.legacy-20260430` 참조.

---

## 한 줄 정의

> **친구 3명이랑 좁은 던전 골목에서, 적 한 마리 한 마리 무겁게 잡으면서, 같이 본 드랍을 분배해 그날의 빌드를 만들어가는 3D 코옵 액션 로그라이크.**

## 머릿속 핵심 그림

> "탱커가 앞에서 방패를 들고 뒤에서 딜러와 힐러가 좁은 던전 골목에서 전투하는 그림"

---

## 🚨 절대 원칙 (Elyqara 폭발 재발 방지 — 사용자 자기 진단 기반)

**진단 (사용자 직인 2026-05-01)**:
> "elyqara가 ai환상이랑 메모리인가? ... 너가 짠 코드들 사이에서 상호작용을 못했어 특히 예전에 짰던 코드들"

**핵심 원흉**: 클로드가 새 기능 추가할 때 옛 코드 안 보고 옆에 새 시스템 만든 것. 같은 카테고리 기능이 여러 패턴으로 흩어져서 코드 카오스 → 사용자 갈피 잃음 → 중단.

### 10 원칙

1. **★ 옛 코드 우선 확인** — 새 기능 추가 전 같은 카테고리 기존 코드 grep/Read 강제. "이미 있는 패턴이 뭐지?" 자문하고 못 찾으면 사용자에게 묻기. 옛 코드 무시는 금지
2. **단일 매니저** — 카테고리당 매니저 1개 (적/스킬/아이템/UI/방 등). 새 기능은 그 안에 들어감. 옆에 새 매니저 만들지 말 것
3. **인터페이스 우선** — 모든 적은 IEnemy, 모든 스킬은 ISkill, 모든 아이템은 IItem 등. 새 코드가 인터페이스 없이 추가되지 못하게 구조
4. **추가 = 데이터 추가** — 새 캐릭/적/아이템이 ScriptableObject 한 장 + 코드 0~1줄로 끝나는 구조. 코드 추가 필요하면 인터페이스 부족 신호
5. **환상 금지** — 코드 추가 전 기존 패턴/파일/타입 실제 확인. 추측해서 만들지 말 것
6. **메모리 즉시 갱신** — 사용자 결정/명확화는 그 턴에 박기. 글로벌 메모리(`~/.claude/projects/-Users-jeong-yunsu/memory/`)와 이 CLAUDE.md 둘 다 갱신
7. **컨벤션 일관성** — 한 번 정한 패턴은 모든 비슷한 케이스에서 동일하게. "예외" 만들지 말 것
8. **YAGNI** — 지금 안 쓰는 추상화/헬퍼/유틸리티 만들지 말 것. 3번 같으면 그때 추출
9. **단일 책임** — 한 파일/클래스가 한 가지만. Elyqara에서 폭발한 신 클래스(StageManager 등) 패턴 반복 금지
10. **★ 정공법 우선 — 빠른 회피 옵션 제시 금지** (사용자 직인 2026-05-01: *"무조건 정공법으로 가 빠른건 그냥 배재해 장기프로젝트야 미래에 어떤 문제도 되지않게 정공법을 기본으로 박아"*)
    - 마찰/문제 만나면 **근본 원인 처리 + 재발 방지 hook 까지가 한 단위.** "임시 회피" / "일단 빠르게" / "지금만 그렇게" — 모두 장기 부채. 금지
    - "방법 A 빠르게 / 방법 B 클린하게" 두 옵션 제시 X — **클린 옵션만 제시.** 사용자가 시간/체력 부족하면 *작업 중단* 이 답. *회피* X
    - 환경 문제도 동일 — "Editor 재시작이 정공" 같은 안내가 회피 아닌 정공이면 그것만 제시. 단 가능하면 코드 레벨 hook 으로 자동화
    - YAGNI(#8) 와 충돌 X. YAGNI = 안 쓰는 추상화 금지. 정공법 = 쓰는 코드의 정합성/재발 방지

### 새 기능 추가 SOP

1. 사용자 요구 받음
2. **외부 자료 검색** — 표준 패턴 (3인칭 카메라 / AI / 인벤 / 네트워킹 등) 은 WebSearch 또는 Explore 로 한 번 검증된 코드 검색. 키워드: "Unity 6000.3 + [기능] + Netcode for GameObjects" / "Cinemachine 3.x + [기능]". 검증된 패턴 발견 시 우선 적용 (사용자 직인 2026-05-02)
3. **관련 카테고리 grep** — "비슷한 거 이미 있나?"
4. **인터페이스/매니저 확인** — "이 카테고리는 어떻게 굴러가지?"
5. 옛 패턴 따라서 추가. 안 맞으면 "왜 안 맞는지" 보고하고 사용자 확인
6. 데이터로 풀 수 있으면 코드 추가 0줄로 끝
7. 메모리 갱신 + 시스템 인덱스 한 줄 추가

### 패키지 추가 SOP (2026-05-01 Facepunch 폭발에서 학습)

> 리서치 결과 ≠ 검증된 사실. 적용 전 컴파일 1회 필수.

1. **한 번에 1개씩 추가** — 여러 패키지 동시 추가 절대 금지. 깬 게 어느 건지 격리 못 함
2. **커뮤니티 / git URL 패키지는 GitHub 최근 커밋 + open issues 확인 후에만**
3. 추가 → Unity 재컴파일 → `~/Library/Logs/Unity/Editor.log` (또는 MCP `read_console`) 으로 컴파일 에러 0건 확인
4. 에러 있으면 **즉시 manifest 에서 제거** (해결 시도 X — 다른 패키지 추가 막힘)
5. 클린 확인 후 다음 패키지

### 리서치 ≠ 검증

- 리서치 에이전트가 가져온 정보는 **가설**. 적용 전 한 번 더 확인.
- "워킹 한다" / "fine for 2026" 같은 표현은 작성자 환경 기준. 우리 Unity 6000.3.7f1 + macOS 24.6 에서 검증되지 않음
- MCP / 에디터가 깨지면 `~/Library/Logs/Unity/Editor.log` 직접 읽기 (deny rule 우회 가능 — 프로젝트 외부)

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

**사용자 직인 2026-05-01**:
> "캐릭터 관련 세부사항은 지금 필요없어 그냥 공격 속성들을 여러개 변수같은거로 만들어둬서 캐릭터 확장성만 높이면돼"

→ 공격 속성을 **변수/데이터**로 추출. 새 캐릭/무기/스킬 = "어떤 속성 가지는지" 데이터로만 명시. 코드 추가 X.

### 데이터 구조 (장기 — B 2 레이어)

```csharp
public enum PhysicalType {
    Slash,   // 참격 — 검, 도끼날
    Blunt,   // 타격 — 방패 슬램, 망치
    Pierce,  // 관통 — 활, 창 (백로그)
}

public enum ElementType {
    None,
    Fire,    // 화염 (백로그)
    Cold,    // 냉기 (백로그)
    Lightning, // 번개 (백로그)
}

[System.Serializable]
public class AttackProperty {
    public PhysicalType physical;
    public ElementType element = ElementType.None;
    public float damageMultiplier = 1f;
}
```

### 1차 (첫 마일스톤)
- 활성 PhysicalType: **Slash, Blunt** 만
- 활성 ElementType: **None** 만
- Pierce / Fire / Cold / Lightning은 enum에는 정의해두지만 사용 X (백로그)

### 캐릭터 = 무기 = 속성 보유 데이터
- 검방패 캐릭 = 검(Slash) + 방패(Blunt) 두 무브셋
- 새 캐릭 추가 = 무기 데이터 + 속성 조합 정의로 끝
- 코드 추가 0줄 지향

### 효과 풀 (1차)
- "Slash 데미지 +X%"
- "Blunt 데미지 +X%"
- 회복 % / 시너지 / 메커니즘 변화 효과는 백로그

---

## 마일스톤 (시간 X, 명제 단위)

### 첫 마일스톤 (코어 검증까지)

> **검증 목표: 검방패 4명이 좁은 던전에서 적 한 마리 무겁게 잡고, 드랍 분배해서 빌드 형성하는 한 런이 재밌는가**

| # | 단계 | 출력 검증 |
|---|---|---|
| 1 | NGO + Steamworks, Capsule 2명 동기화 | "둘이 같이 움직임" |
| 2 | 검방패 캐릭터 완성 (무브 + 스킬) | "허공 휘둘러도 무게 있음. 캐릭터 완성됨" |
| 3 | 더미 적 1종 + 한 마리 집중 AI | "혼자 적 한 마리 잡는 게 재밌음" |
| 4 | 2~4명 멀티 동기화 | "같이 잡는 게 더 재밌음" |
| 5 | 좁은 던전 1방 (단일) | "좁은 공간 포지셔닝 의미 있음" |
| 6 | 던전 확장 (방+통로, **C 하이브리드** Elyqara 그리드 3D 포팅) | "한 런 길이 감" |
| 7 | 빌드 시스템 (Elyqara 그리드 인벤토리 차용) | "적 잡으면 뭔가 떨어짐" |
| 8 | 자유 픽업 드랍 (그냥 바닥) | "협동의 본질이 굴러감" |
| 9 | 다운/부활 | "한 명 죽어도 끝 아님" |
| 10 | 보스 + 한 런 사이클 | "한 런이 완전 굴러감" |
| **11** | **🎯 코어 검증 플레이테스트 (친구 4명)** | "재밌는가? 다시 하고 싶은가?" |

### 코어 검증 통과 후 (큰 그림만)
- M12 콘텐츠 확장 (적/아이템/던전 다양성)
- M13 외형 정체성 (첫 캐릭 모델/애니/이름/VFX)
- M14 폴리싱
- M15 Steam 페이지 + 출시

**기간 중요 X — 명제 만족 시점이 검증.** 호텔 일 + N1 공부 + 게임 사이드 강등 정합 (사용자 직인: "주차별말고 그냥 순서로 해줘 빨리개발하는달이랑 늦게 개발하는 달이 달라서").

---

## 첫 마일스톤 백로그 (포함 X)

- 마스터리 / 메타 통화 / 영구 진행
- 회복 % / 시너지 발동 / 메커니즘 변화 효과
- 원소 속성 (Fire/Cold/Lightning) / Pierce
- 두 번째 캐릭 (Tier 1 코만도풍 1캐릭 4명)
- 허브 / 마스터리 화면 / 캐릭터 선택
- 외형 정체성 (모델/애니/이름/VFX)
- 도발 스킬 / 정밀 어그로 시스템 (자율적 역할 안에서 자연 발생만)

---

## 기술 스택

- **엔진**: Unity 6000.3.7f1
- **네트워킹**: Unity Netcode for GameObjects (NGO)
- **트랜스포트**: Steamworks (Facepunch.Steamworks)
- **권한 모델**: 호스트 권한 (Host-authoritative)
- **절차생성 동기화**: 시드 broadcast → 클라가 결정론적 생성
- **아트**: 사용자가 직접 찾고 결정 (Quaternius/Kenney/Mixamo 등은 옵션 안내만)

### 멀티 우선 아키텍처 (1주차부터)
1. 모든 게임 상태는 호스트 권위
2. 절차생성은 시드 동기화
3. 싱글플레이 = "혼자 호스트"하는 케이스. 오프라인 모드 따로 X

---

## 시스템 인덱스 (코딩 진행하면서 한 줄씩 추가)

> 새 시스템 추가될 때마다 한 줄 추가. 새 세션 클로드가 빠르게 파악 가능하도록.

### 현재 상태 (2026-05-01)
- **단계 1 ✅** "둘이 같이 움직임"
- **단계 2 ✅** "캐릭터 슬롯/자원 그릇 완성" — MPPM 두 캡슐 + 어깨 너머 카메라 + owner 분리 통과
- **단계 3 ✅** "혼자 적 한 마리 잡는 게 재밌음" — Wisp 좌클릭 4회 사망 + Souls 톤 카메라 검증 통과. 사용자 직인 *"저것까지 제대로 작동하네"*
- **코드**: `Assets/_Project/Scripts/{Networking,Player,Characters}/` (asmdef 3개 분리)
- **프리팹**: `Player.prefab` (root: Capsule + Rigidbody + NetworkObject + NetworkTransform + NetworkRigidbody + PlayerMovement + PlayerInput + PlayerCharacterBinder + PlayerResources + PlayerCamera) + 자식 `vCam` (CinemachineCamera + ThirdPersonFollow)
- **씬**: `SampleScene` 에 `[Network]` (NetworkManager + UnityTransport + NetworkBootstrap) + `Ground` Plane + Main Camera 에 CinemachineBrain
- **데이터**: `Assets/_Project/Data/Characters/Kiyan.asset` (첫 캐릭 SO. 슬롯 4개 비어있음)
- **폴더 스켈레톤**: `Assets/_Project/{Scripts/{Networking,Player,Characters},Data/Characters,Prefabs/Networking,Scenes,Settings,Art,Audio}/`
- **GitHub**: `https://github.com/comdbstn/elyqara_3D` main 브랜치
- **Steam AppID**: 480 (Spacewar) — dev 테스트용. 실제 출시 전 교체 필수
- **MCP**: CoplayDev/unity-mcp 9.6.8 등록 완료. `claude mcp list` 결과 = `unity-mcp: http://127.0.0.1:8080/mcp (HTTP) - ✓ Connected`. Unity 켜져있는 동안만 작동 (서버가 Unity 안에서 돌아감). 첫 도구 호출 전 항상 `claude mcp list` 로 검증 권장
- **Steam P2P 트랜스포트**: 보류. 단계 1 은 NGO 기본 UTP(Unity Transport)로 LAN 테스트. 커뮤니티 Facepunch 트랜스포트는 컴파일 깨짐 확인 (FacepunchTransport.cs:288 CS1028) → 단계 1 검증 후 별도 솔루션

### 패키지 — manifest.json (각 역할)

| 패키지 | 버전 | 역할 |
|---|---|---|
| `com.coplaydev.unity-mcp` | git/main | Claude ↔ Unity MCP 브릿지. Editor 안에서 도구 호출 받음 |
| `com.unity.netcode.gameobjects` | 2.4.4 | NGO 2.x — 호스트 권한 네트워킹. NGO 1.x는 6000.3 deprecated |
| `com.unity.cinemachine` | 3.1.6 | CM3 — 3인칭 추격 카메라. **API 가 CM2와 다름** (`CinemachineCamera`) |
| `com.unity.render-pipelines.universal` | 17.3.0 | URP — Forward+ 추천. 모바일 렌더러도 같이 들어가있음 |
| `com.unity.inputsystem` | 1.18.0 | New Input System. legacy `Input.GetAxis` 대체 |
| `com.unity.ai.navigation` | 2.0.9 | NavMesh 빌더 — 적 AI 경로 |
| `com.unity.multiplayer.center` | 1.0.1 | NGO 셋업 위저드. 안 써도 무방 |
| `com.unity.multiplayer.playmode` | 2.0.2 | **MPPM** — Editor 안에서 Player 1 + Player 2 동시 띄움 (가상 인스턴스). 단계 1 검증 도구. 단계 4 멀티 동기화 검증에도 핵심 |
| `com.unity.test-framework` | 1.6.0 | Unit/Integration 테스트. `run_tests` MCP 도구 의존 |
| `com.unity.timeline` | 1.8.10 | Timeline — 컷씬용. 백로그 (1차 안 씀) |
| `com.unity.ugui` | 2.0.0 | uGUI — HUD/네임플레이트는 uGUI 권장 (UI Toolkit 보다 월드스페이스 안정적) |
| `com.unity.visualscripting` | 1.9.9 | Bolt — 안 씀. 컴파일 시간 영향. 1차 끝나고 정리 후보 |
| `com.unity.collab-proxy` | 2.11.3 | Plastic SCM — 안 씀. 정리 후보 |
| `com.unity.ide.rider/visualstudio` | - | IDE `.csproj` 자동 생성. JetBrains Rider 또는 VS 쓰면 유지 |
| `com.unity.modules.*` | 1.0.0 | Unity 빌트인 모듈 (Animation, Physics, Audio, Particles 등). 손대지 말 것 |

**아직 추가 안 함 (단계별 추가):**
- ProBuilder (단계 6 던전 그레이박스)
- Animation Rigging (단계 2 검방패 IK)
- TextMeshPro — 6000.3에서 빌트인 모듈로 흡수됨 (별도 추가 불필요)

### 시스템 도구 상태 (2026-05-01 검증)

| 도구 | 상태 | 경로 |
|---|---|---|
| Homebrew | 5.1.0 ✅ | `/opt/homebrew/bin/brew` |
| Python 3 | 3.12.13 ✅ | `/opt/homebrew/bin/python3` (symlink → 3.12.13) |
| uv | 0.11.8 ✅ | `/opt/homebrew/bin/uv` |
| Node | 24.8.0 ✅ | (gamelovers MCP 호환용, CoplayDev 가 활성화되면 의존성 X) |
| Unity | 6000.3.7f1 ✅ | `/Applications/Unity/Hub/Editor/6000.3.7f1` |
| Claude Code | 활성 ✅ | (이 세션) |
| /etc/paths.d/homebrew | ✅ | `/opt/homebrew/bin` GUI 앱 PATH 에 자동 포함 |

### 카테고리별 인덱스 (단계 진행하면서 채움)
> 형식: **카테고리**: 인터페이스 + 매니저 + 데이터 패턴

- **네트워킹** ✅ 단계 1+2: `Elyqara.Networking` asmdef. `NetworkBootstrap` (Host/Client/Server OnGUI + UnityTransport 자동 link + PlayMode Stop 시 강제 Shutdown hook). 단계 2부터 PlayerMovement 는 Player asmdef 로 이동 — Networking 은 진짜 인프라(트랜스포트/부트스트랩)만. (단계 1 후) SteamLobbyService 추가 예정
- **플레이어** ✅ 단계 2: `Elyqara.Player` asmdef. `PlayerInput` (Move/Look + 4슬롯 InputAction, Owner-only enable). `PlayerMovement : NetworkBehaviour` (IsOwner 입력 → ServerRpc → 호스트 Rigidbody.linearVelocity → NetworkTransform 동기화). `PlayerResources : NetworkBehaviour` (NetworkVariable<float> Health/Stamina, 호스트 권위, regen). `PlayerCamera : NetworkBehaviour` (vCam SetParent(null) 분리 + Priority.Value 명시). `PlayerCharacterBinder` (CharacterData ref 한 줄 컴포넌트)
- **캐릭터** ✅ 단계 2: `Elyqara.Characters` asmdef. `CharacterData : ScriptableObject` (HP/Stamina + 4 슬롯). 새 캐릭 추가 = SO 한 장 + Player.prefab 의 PlayerCharacterBinder.character 한 줄 변경. 첫 캐릭 = Kiyan.asset
- **적** ✅ 단계 3: `Elyqara.Enemies` asmdef. `IEnemy` + `EnemyData : ScriptableObject` (HP/속도/어그로/공격 데이터) + `EnemyController : NetworkBehaviour` FSM 3상태 (Idle/Chase/Attack with windup) + `EnemySpawner` (Host 시작 시 1마리 spawn). 새 적 추가 = SO 한 장 + prefab 한 장 + NetworkPrefabsList 자동 등록. 첫 적 = `Wisp.asset` (HP 120). 단계 4 멀티에서 다중 spawn 시 EnemyManager 로 승격
- **스킬** ✅ 단계 3: `Elyqara.Skills` asmdef. `ISkill` + `SkillData : ScriptableObject, ISkill` (추상) + `BasicMeleeSkill` (구체 — 콘 hitbox 데미지) + `IDamageable` (Player/Enemy 둘 다 구현). 새 스킬 = SkillData 상속 SO 한 장. `PlayerSkillExecutor` 가 4슬롯 입력 → `SkillData.ActivateOnServer` 호출. 첫 스킬 = `BasicMelee.asset`
- **아이템**: IItem + InventoryManager (Elyqara 그리드 차용 — 단계 7)
- **던전**: GridMapGenerator → GridDungeonBuilder → GridRoomManager (Elyqara 패턴 — 단계 5/6)
- **공격 속성**: AttackProperty 데이터 구조. 캐릭/무기/스킬/효과가 참조 (단계 5+)

---

## Unity MCP + Claude Code 워크플로우 (커뮤니티 검증 패턴)

> "옛 코드 우선 확인" 10 원칙을 도구 호출 순서로 박은 SOP. 단계마다 빼먹지 말 것.

```
1. SEARCH (9 원칙 #1)
   → Grep / Glob / code-pattern-checker 에이전트로 옛 패턴 조사
   → 못 찾으면 사용자에게 물어봄 (추측 금지)

2. PLAN
   → 데이터로 끝나나? (ScriptableObject 한 장)
   → 새 코드 필요하면 어느 매니저/인터페이스 안인가?

3. EDIT
   → 새 .cs: Write 도구
   → 기존 .cs 수정: Edit 도구 (GUID 안전)
   → 씬/프리팹 변경: mcp__unity-mcp__* (ask 권한)

4. RECOMPILE
   → mcp__unity-mcp__recompile_scripts

5. L1 — 컴파일 검증
   → mcp__unity-mcp__get_console_logs (errors only)
   → 에러 있으면 → 3번으로 복귀

6. L2 — 런타임 검증 (필요할 때만)
   → mcp__unity-mcp__execute_menu_item "Edit/Play"
   → get_console_logs (런타임 예외)
   → execute_menu_item "Edit/Play" 다시 (정지)

7. SAVE (씬/프리팹 변경 시만)
   → mcp__unity-mcp__save_scene

8. 메모리 갱신 (9 원칙 #6)
   → 시스템 인덱스에 한 줄 추가
   → CLAUDE.md / 글로벌 메모리 둘 다
```

### 도구 사용 규칙

- **읽기만(allow)**: get_console_logs / get_scene_info / get_gameobject / recompile_scripts / run_tests / select_gameobject / send_console_log
- **쓰기(ask 강제)**: 씬·프리팹·머터리얼·트랜스폼·컴포넌트 변경 전부. 사용자 확인 받은 뒤 실행
- **삭제(deny)**: delete_gameobject / delete_scene 비활성. 정말 필요하면 사용자가 직접 Unity에서
- **Library/Temp/Logs/Build/obj/UserSettings 읽기 deny**: 토큰 낭비 방지

### 알려진 함정

- `*.meta` 파일 절대 git에서 제거 금지 — GUID 깨지면 프리팹 사일런트 파괴
- NGO 1.x 튜토리얼 따라가지 말 것 — 6000.3에서 deprecated. 2.x API 다름
- Cinemachine 2.x 코드 (`CinemachineVirtualCamera`) 쓰지 말 것 — 3.x는 `CinemachineCamera`
- Unity 가 컴파일 중일 때 `save_scene` 호출 금지 — race condition. 항상 recompile_scripts → console 클린 확인 → save 순서
- macOS Finder에서 Unity 띄우면 PATH 안 넘어감 — Terminal에서 `open -a Unity` 권장

---

## Elyqara (2D)와의 관계

`/Users/jeong-yunsu/Elyqara/`는 1주짜리 바이브코딩 프로토타입. 핵심 로직 좋다고 본인 판단 → 같은 IP/이름 사용.

### KEEP
- 판타지 이세계 세계관 + Elyqara 이름/IP
- 검사 캐릭터 컨셉 (검방패로 구체화)
- Forest 바이옴 톤 (3D 재해석은 백로그)
- 던전 크롤러 + 로그라이크 구조

### CUT
- v4 88퍽 / 15세트 / 16 Legendary / 보석 / 5트리×5티어 / 8종 특수방 / 시너지 → 제거 (복잡도 폭발 원인)
- 4 캐릭터 → 1 캐릭터
- 4 스테이지 → 1 던전

### NEW
- 3D 3인칭 어깨 너머
- 자율적 역할구조 (트리니티 그림은 살아있되 빌드로 형성)
- 호스트 권한 멀티 (NGO + Steam)
- 무기 캐릭터별 고정 + RPG 아이템 빌드 (BG3/바간테식)
- 다운→부활
- 다크소울/MH급 무거운 전투

### Elyqara 코드 활용 규칙
- `/Users/jeong-yunsu/Elyqara/`는 **read-only 레퍼런스**. 직접 포팅 X
- 패턴 참조 + 데이터 추출만 허용

가장 가치 있는 패턴 (3D 포팅 시 참조):
1. 데미지 파이프라인 (`DamageCalculator.cs`, `RuntimeStatHolder.cs`)
2. 적 AI FSM
3. 그리드 인벤토리 패턴 (단계 7 차용)
4. 그리드 던전 파이프라인 (단계 6 차용)
5. 세이브 시스템 (백로그)

---

## 커리어 컨텍스트 (사이드 강등)

- **메인 트랙(비자→MBA→고도인재)이 우선**, 게임은 사이드 자산
- **단기**: 데모 출시 → 일본 게임업계 포트폴리오
- **중기**: Day 1부터 유튜브/X 공개 → 콘텐츠 자산
- **장기**: SAO급 3D VR RPG 궁극 목표의 첫 마일스톤

**Day 1 콘텐츠 전략**: 매주 dev log 업로드. 회색 캐릭 시기 = "정체성 없이 게임 만드는 과정" 자체가 콘텐츠.

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
- [ ] 빠른 회피 옵션 제시했나? (★) — 클린 옵션만 제시. "임시" 우회 박힌 코드/설정 없는지 확인
- [ ] 마찰 만난 부분에 재발 방지 hook 박혔나? (★)

---

## 결정 로그

### 2026-04-30 (legacy)
- 옛 비전 (트리니티 메커니즘 + 시작 픽 빌드 + 인게임 강화 미기획)
- 상세는 `CLAUDE.md.legacy-20260430` 참조
- 5-1 대화에서 비전 갈아엎힘

### 2026-05-01 비전 풀 락
- **장르 톤**: MH/다크소울 톤. RoR2/MH 모순은 종결 (RoR2에서 차용은 캐릭터 선택 자유 + 자율적 역할구조 2가지만)
- **빌드 시스템**: BG3/바간테식 RPG 아이템 (랜덤 드랍, 무기+방어구, Elyqara 그리드 차용). 옛 "인게임 강화 미기획" 폐기
- **공격 속성**: 데이터 구조로 추출 (B 2 레이어, 1차 Slash/Blunt). 캐릭터 확장성 핵심
- **무기**: 캐릭터별 고정. 첫 캐릭 검방패 (검=Slash, 방패=Blunt)
- **던전**: C 하이브리드 (Elyqara 그리드 3D 포팅)
- **분배**: 자유 픽업 (그냥 바닥)
- **인원**: 1~4명 (옛 1·2·4 폐기)
- **마일스톤**: 11단계 코어 검증 (옛 M1~M10 폐기)
- **시간 단위 X**: 순서로만 표기
- **절대 원칙**: 9 원칙 + SOP (옛 코드 우선 / 단일 매니저 / 인터페이스 우선 / 데이터 주도 / 환상 금지)

### 2026-05-01 단계 2 통과 + 정공법 우선 원칙 박힘
- **단계 2 ✅** "캐릭터 슬롯/자원 그릇 완성" — MPPM 두 캡슐 + 어깨 너머 카메라 + owner 분리 + 콘솔 0/0/0 통과
- **카테고리 추가 (asmdef 분리)**: `Elyqara.Characters` (CharacterData SO 그릇), `Elyqara.Player` (Input/Movement/Resources/Camera/CharacterBinder). `Elyqara.Networking` 은 NetworkBootstrap 만 남기고 슬림화 (네트워크 인프라 책임)
- **첫 캐릭**: `Assets/_Project/Data/Characters/Kiyan.asset` — HP 100, Stamina 100, regen 15/sec, 슬롯 4개(primary/secondary/qSkill/dodge) 비어있음. **새 캐릭 추가 = SO 한 장 옆에 + Player.prefab 의 PlayerCharacterBinder.character 한 줄 변경**
- **스킬 슬롯 입력 바인딩 (사용자 직인)**: 마우스 좌클릭 = primary, 마우스 우클릭 = secondary, Q = qSkill, Space = dodge. 동작 구현 X (단계 5+ ISkill 정의 시 채움)
- **카메라**: Cinemachine 3 어깨 너머. vCam 은 Player 자식이지만 OnNetworkSpawn 에서 `SetParent(null)` 로 분리 (부모 transform 변환 + ThirdPersonFollow 자체 계산 이중 적용 방지). Priority 는 PrioritySettings struct 의 `.Value` 명시 박기. ShoulderOffset (0.5, 0.2, 0) / VerticalArmLength 0.4 / CameraSide 1 / CameraDistance 4
- **함정 학습**: NGO UnityTransport 의 PlayMode Stop 시 UDP socket 좀비 — `NetworkManager.Shutdown(true)` 도 native socket release 못 하는 케이스. NetworkBootstrap 에 EditorApplication.playModeStateChanged 콜백으로 강제 정리 hook 박음. 좀비 발생 시 Editor 재시작이 환경 레벨 정공
- **★ 절대 원칙 #10 정공법 우선 박힘** (사용자 직인): *"무조건 정공법으로 가 빠른건 그냥 배재해 장기프로젝트야 미래에 어떤 문제도 되지않게 정공법을 기본으로 박아"* / *"가자 나한테 확인해줄거 말하면 무조건 내가 해줄게 난 너 믿고 너도 나 믿지?"*
- 직전 트리거: 7777 socket 좀비 만나서 클로드가 "방법 A 빠르게(7778 임시) / 방법 B 클린하게(Editor 재시작)" 두 옵션 제시 → 사용자 거부. 7778 임시 변경 → 7777 복구 + cleanup hook 박는 정공으로 회귀
- 검증 룰 박힘: 매 단계 명제는 사용자가 직접 플레이 검증 후 직인. 클로드는 "확인해줄거" 명시하면 사용자가 검증 응답. 검증 통과 전 다음 단계 진입 금지

### 2026-05-01 단계 1 통과 + 권한 모델 변경
- **단계 1 ✅** "둘이 같이 움직임" MPPM 검증 통과. 사용자 직인 "ㅈㄴ만족함"
- **GitHub 박힘**: `https://github.com/comdbstn/elyqara_3D` (main, push 자동)
- **권한 게이트 제거**: `.claude/settings.json` 의 모든 ask → allow. deny 는 후처리 불가능한 destructive 만 (rm -rf, git reset --hard, push --force, branch -D 등). 사용자 결정 — review 능력 부재로 ask 마찰 무의미. **Claude self-discipline 가중** (9 원칙 + SOP 자동 self-check)
- **Server 버튼 유지 결정**: NetworkBootstrap 의 Server 버튼은 의도적 유지. 다음 세션이 다시 빼라고 제안 X. 단계 후반 로비 UI 만들면 자연 제거됨
- **HANDOFF.md 삭제**: 단계 1 직전 상태 인계 문서. 쓰임 다 함

### 2026-05-02 단계 3 통과 + 외부 자료 활용 원칙 박힘
- **단계 3 ✅** "혼자 적 한 마리 잡는 게 재밌음" — Wisp 좌클릭 4회 사망 + Souls 톤 카메라 검증 통과
- **사용자 직인**: *"저것까지 제대로 작동하네"*
- **외부 자료 활용 원칙 박힘** (사용자 직인 2026-05-02): *"이런 3인칭 카메라 뷰같은것들은 인터넷이나 깃에도 코드있을것같은데 앞으로 개발할때는 인터넷 자료들 최대한 활용하면서 가자"*
- 새 SOP 단계 2 (외부 자료 검색) 추가. 표준 패턴은 인터넷 검증된 코드 우선
- **카메라 fix 핵심 학습**:
  - CM 3.x `CinemachineThirdPersonFollow` extension 이 NetworkBehaviour + SetParent(null) 환경에서 Tracking Target *위치도 회전도* 추적 안 됨. 자체 진단 시간 폭발 (외부 자료 검색 안 한 손실)
  - 정공: `PlayerCamera` 가 LateUpdate 에서 vCam.transform 직접 박는 패턴 (yaw = Player.yaw / pitch = 마우스 Y 자체 누적). ThirdPersonFollow 컴포넌트 disable 안전망
  - Souls 톤 값: distance 5.5 / vertical 1.6 / horizontal 0 / pitch -10~50 / pitchSensitivity 0.1
- **InputSystem 함정 학습**:
  - Mouse delta read 시 `InputActionType.Value` 가 ReadValue 0 read 하는 케이스 — `InputActionType.PassThrough` + `expectedControlType: "Vector2"` 가 표준
- **NGO 환경 함정**:
  - UnityTransport 의 PlayMode Stop 시 7777 UDP socket 좀비 — `NetworkManager.Shutdown(true)` 도 native release 못 함. **Unity Editor 재시작이 환경 정공**. NetworkBootstrap PlayMode-stop hook 박혀있어도 NGO 한계
  - MPPM Player 2 코드 변경 자동 reload 안 받음 — 변경 후 OFF/ON 토글 필수
- **백로그**: vCam prefab 의 ThirdPersonFollow 컴포넌트 자체 제거 (현재 disable 안전망). MCP 부활 시 처리
