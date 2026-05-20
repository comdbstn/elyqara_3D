# 결정 로그

> 단계별 사용자 직인 + 핵심 결정 + 함정 학습 기록. 새 결정은 끝에 추가. CLAUDE.md 비대화 방지를 위해 분리.

---

## 2026-04-30 (legacy)
- 옛 비전 (트리니티 메커니즘 + 시작 픽 빌드 + 인게임 강화 미기획)
- 상세는 `CLAUDE.md.legacy-20260430` 참조
- 5-1 대화에서 비전 갈아엎힘

## 2026-05-01 비전 풀 락
- **장르 톤**: MH/다크소울 톤. RoR2/MH 모순은 종결 (RoR2에서 차용은 캐릭터 선택 자유 + 자율적 역할구조 2가지만)
- **빌드 시스템**: BG3/바간테식 RPG 아이템 (랜덤 드랍, 무기+방어구, Elyqara 그리드 차용). 옛 "인게임 강화 미기획" 폐기
- **공격 속성**: 데이터 구조로 추출 (B 2 레이어, 1차 Slash/Blunt). 캐릭터 확장성 핵심
- **무기**: 캐릭터별 고정. 첫 캐릭 검방패 (검=Slash, 방패=Blunt)
- **던전**: C 하이브리드 (Elyqara 그리드 3D 포팅) ← 단계 6 진입 시 *직접 맵핑* 으로 변경됨
- **분배**: 자유 픽업 (그냥 바닥)
- **인원**: 1~4명 (옛 1·2·4 폐기)
- **마일스톤**: 11단계 코어 검증 (옛 M1~M10 폐기)
- **시간 단위 X**: 순서로만 표기
- **절대 원칙**: 9 원칙 + SOP (옛 코드 우선 / 단일 매니저 / 인터페이스 우선 / 데이터 주도 / 환상 금지)

## 2026-05-01 단계 1 통과 + 권한 모델 변경
- **단계 1 ✅** "둘이 같이 움직임" MPPM 검증 통과. 사용자 직인 "ㅈㄴ만족함"
- **GitHub 연결**: `https://github.com/comdbstn/elyqara_3D` (main, push 자동)
- **권한 게이트 제거**: `.claude/settings.json` 의 모든 ask → allow. deny 는 후처리 불가능한 destructive 만 (rm -rf, git reset --hard, push --force, branch -D 등). 사용자 결정 — review 능력 부재로 ask 마찰 무의미. **Claude self-discipline 가중** (10원칙 + SOP self-check)
- **Server 버튼 유지 결정**: NetworkBootstrap 의 Server 버튼은 의도적 유지. 다음 세션이 다시 빼라고 제안 X. 단계 후반 로비 UI 만들면 자연 제거됨
- **HANDOFF.md 삭제**: 단계 1 직전 상태 인계 문서. 쓰임 다 함

## 2026-05-01 단계 2 통과 + 정공법 우선 원칙 추가
- **단계 2 ✅** "캐릭터 슬롯/자원 그릇 완성" — MPPM 두 캡슐 + 어깨 너머 카메라 + owner 분리 + 콘솔 0/0/0 통과
- **카테고리 추가 (asmdef 분리)**: `Elyqara.Characters` (CharacterData SO 그릇), `Elyqara.Player` (Input/Movement/Resources/Camera/CharacterBinder). `Elyqara.Networking` 은 NetworkBootstrap 만 남기고 슬림화 (네트워크 인프라 책임)
- **첫 캐릭**: `Assets/_Project/Data/Characters/Kiyan.asset` — HP 100, Stamina 100, regen 15/sec, 슬롯 4개(primary/secondary/qSkill/dodge) 비어있음. **새 캐릭 추가 = SO 한 장 + Player.prefab 의 PlayerCharacterBinder.character 한 줄 변경**
- **스킬 슬롯 입력 바인딩 (사용자 직인)**: 마우스 좌클릭 = primary, 마우스 우클릭 = secondary, Q = qSkill, Space = dodge. 동작 구현 X (단계 5+ ISkill 정의 시 채움)
- **카메라**: Cinemachine 3 어깨 너머. vCam 은 Player 자식이지만 OnNetworkSpawn 에서 `SetParent(null)` 로 분리 (부모 transform 변환 + ThirdPersonFollow 자체 계산 이중 적용 방지). Priority 는 PrioritySettings struct 의 `.Value` 명시. ShoulderOffset (0.5, 0.2, 0) / VerticalArmLength 0.4 / CameraSide 1 / CameraDistance 4
- **함정 학습**: NGO UnityTransport 의 PlayMode Stop 시 UDP socket 좀비 — `NetworkManager.Shutdown(true)` 도 native socket release 못 하는 케이스. NetworkBootstrap 에 EditorApplication.playModeStateChanged 콜백으로 강제 정리 hook 추가. 좀비 발생 시 Editor 재시작이 환경 레벨 정공
- **★ 절대 원칙 #10 정공법 우선 추가** (사용자 직인): *"무조건 정공법으로 가 빠른건 그냥 배재해 장기프로젝트야 미래에 어떤 문제도 되지않게 정공법을 기본으로 박아"* / *"가자 나한테 확인해줄거 말하면 무조건 내가 해줄게 난 너 믿고 너도 나 믿지?"*
- 직전 트리거: 7777 socket 좀비 만나서 클로드가 "방법 A 빠르게(7778 임시) / 방법 B 클린하게(Editor 재시작)" 두 옵션 제시 → 사용자 거부. 7778 임시 변경 → 7777 복구 + cleanup hook 추가 정공으로 회귀
- 검증 룰 추가: 매 단계 명제는 사용자가 직접 플레이 검증 후 직인. 클로드는 "확인해줄거" 명시하면 사용자가 검증 응답. 검증 통과 전 다음 단계 진입 금지

## 2026-05-02 단계 3 통과 + 외부 자료 활용 원칙 추가
- **단계 3 ✅** "혼자 적 한 마리 잡는 게 재밌음" — Wisp 좌클릭 4회 사망 + Souls 톤 카메라 검증 통과
- **사용자 직인**: *"저것까지 제대로 작동하네"*
- **외부 자료 활용 원칙 추가** (사용자 직인 2026-05-02): *"이런 3인칭 카메라 뷰같은것들은 인터넷이나 깃에도 코드있을것같은데 앞으로 개발할때는 인터넷 자료들 최대한 활용하면서 가자"*
- 새 SOP 단계 2 (외부 자료 검색) 추가. 표준 패턴은 인터넷 검증된 코드 우선
- **카메라 fix 핵심 학습**:
  - CM 3.x `CinemachineThirdPersonFollow` extension 이 NetworkBehaviour + SetParent(null) 환경에서 Tracking Target *위치도 회전도* 추적 안 됨. 자체 진단 시간 폭발 (외부 자료 검색 안 한 손실)
  - 정공: `PlayerCamera` 가 LateUpdate 에서 vCam.transform 직접 갱신 (yaw = Player.yaw / pitch = 마우스 Y 자체 누적). ThirdPersonFollow 컴포넌트 disable 안전망
  - Souls 톤 값: distance 5.5 / vertical 1.6 / horizontal 0 / pitch -10~50 / pitchSensitivity 0.1
- **InputSystem 함정 학습**:
  - Mouse delta read 시 `InputActionType.Value` 가 ReadValue 0 read 하는 케이스 — `InputActionType.PassThrough` + `expectedControlType: "Vector2"` 가 표준
- **NGO 환경 함정**:
  - UnityTransport 의 PlayMode Stop 시 7777 UDP socket 좀비 — `NetworkManager.Shutdown(true)` 도 native release 못 함. **Unity Editor 재시작이 환경 정공**. NetworkBootstrap PlayMode-stop hook 있어도 NGO 한계
  - MPPM Player 2 코드 변경 자동 reload X — 변경 후 OFF/ON 토글 필수
- **백로그**: vCam prefab 의 ThirdPersonFollow 컴포넌트 자체 제거 (현재 disable 안전망)

## 2026-05-02 단계 4 진행 — 적 AI 표준 패턴 갈아엎힘
- **트리거**: 단계 4 검증 중 Wisp 가 첫 공격 후 두번째부터 Player 한테 *꼴아박기만* 함. Claude ad-hoc FSM 3상태 (Idle/Chase/Attack with single windup) 가 두번째 공격 막힌 원인을 코드만 읽고 짚지 못함
- **사용자 직인** (진단 부족 지적): *"진짜 문제는 이 문제가 아니라 이런 간단한 문제를 너가 컨트롤 하지 못한다는거야. 너가 100% 이해하는 코드로 짜와. 다른 모든코드를 검토하면 이런 간단한 문제정도는 알수있어 너 그렇게 멍청하지않아."*
- **결정**: WebSearch 로 Souls-like 표준 패턴 검색 → EnemyController 전체 갈아엎기. ad-hoc FSM 의 빈 곳 4개 진단:
  1. **Recovery phase 부재** — windup → 데미지 → cooldown 동안 forward 박치기 (= "꼴아박는다" 의 원인)
  2. **Stopping distance 부재** — Chase 중 매 프레임 forward velocity 추가 → capsule 충돌 jitter
  3. **Update 에서 Rigidbody.linearVelocity 갱신** — FixedUpdate 가 표준
  4. **`_state =` 직접 변경하는 곳 4군데로 분산** — frame-by-frame 추적 불가
- **새 베이스 (모든 적 공통)**: 6-state FSM (Idle/Chase/Anticipation/Active/Recovery/Dead) + Transition() 단일 진입점 + 4-phase attack + Stopping Distance + 콘 hitbox. *데이터 주도* — 새 적 = EnemyData SO + prefab 한 장
- **사용자 직인** (통과 후): *"괜찮네. 이렇게 인터넷 찾아보니까 너도 만족스러운 코드가 나오지?"*
- **★ 메타 학습**: 표준 패턴 = 검증된 사람들이 *풀 추적 가능하게 분리* 해놓은 구조 = *Claude 본인의 추적 가능성* 까지 향상. 외부 자료 = 임시방편 X, *정공*. SOP 단계 2 (외부 자료 검색) 를 *첫 코드 작성 전* 진짜로 수행. *임시 코드 → 검증 → 갈아엎기* 사이클 금지 — 시간/체력 + 사용자 신뢰 손상

## 2026-05-02 단계 5 통과 + 추측 금지 hook 추가
- **단계 5 ✅** "좁은 공간 포지셔닝 의미 있음" — ProBuilder 6.0.9 + StartRoom prefab (ProBuilder open box + 4면 벽 + 입구 + 내부 obstacle, EnemySpawner 자식 흡수)
- **사용자 직인** (재미): *"됐다 씨 벌써재밌다 난 평생 3d만 만들래"* + *"5단계 여기서 끝내고 6단계 들어가자"*
- **★ ProBuilder 6.0.4 추측 폭발 학습 (재발 방지 hook 추가)**:
  - Claude 가 6.0.4 *"안전한 보수 stable"* 추측 추가 → Compilation Pipeline fatal crash 두 번
  - WebSearch 결과에 6.0.4 source 0건이었는데도 추가 = 환상 (절대 원칙 #5 위반)
  - 사용자 직인: *"진짜 문제는 이 문제가 아니라 이런 간단한 문제를 너가 컨트롤 하지 못한다는거야"* + *"가장중요해 저건 추측안하기"* + *"저거 뒤로가면 복잡해져서 하나만 실수해도 프로젝트 꼬여서 저번처럼 전체폐기해야될수도있어"*
  - **Hook 추가 (4단)**:
    1. 절대 원칙 #5 강화 — 값/버전/ID/임계값 포함, Citation-first, 위험 신호 인지 (*"안전한 보수"* / *"likely correct"* 등 = 환상 가면), Mid-response retraction, 불확실성 인정 우선
    2. 패키지 추가 SOP 강화 — Unity 공식 패키지는 Package Manager UI 통한 사용자 설치 부탁. manifest 임의 박기 금지 (메모리 검증된 버전 예외만)
    3. 환경 fix SOP — PackageCache 손상시 정공 (`rm -rf Library/PackageCache/com.unity.[패키지명]@*` 또는 Library 전체)
    4. 새 메모리 `feedback_no_guessing_values.md` — Anthropic 공식 anti-hallucination 5 룰 통합 (Citation-first, 불확실성 인정 우선, Mid-response retraction, 위험 신호 인지, 진행 추진력 ≠ 검증 생략)
- **Unity 6 ProBuilder 워크플로우 학습**: Tools > ProBuilder > Editors > Create Shape > Cube. Window > ProBuilder Window 가 Unity 6 디자인 변경으로 사라짐 (contextual menu + scene view toolbar 통합)

## 2026-05-02 단계 6 통과 + 톤 피드백 (★ "박음/박는" 표현 자제)
- **단계 6 ✅** "한 런 길이 감" — 직접 맵핑 + 4방향 Door socket 정렬 + 7-room 자동 생성 도구
- **사용자 직인**: *"맵은 정상적으로 생성됐고"* + *"오옹이 된듯?"* (Gizmo 시각화 통과)
- **kickoff 가설 폐기 → 직접 맵핑 채택**: 사용자 직인 단계 6 진입 직후 *"Elyqara의 그리드처럼 가진 않을거야. 오히려 던전본느낌이야. 그냥 방이 붙어있는거야. 중간에 좁은 복도들은 일단 없이 가도 될것같아. 방마다 문 위치만 4방향 통일시키면 현실감 있는 맵이 나올것같아."* + *"elyqara는 맵 프로그래밍으로 생성하는건데, 이거는 직접 맵핑하는거라 많이 달라"*
  - 폐기: 절차생성, 시드 broadcast, 통로 prefab, Elyqara 2D 그리드 패턴 3D 포팅
  - 채택: 직접 맵핑, 4방향 Door socket 정렬, 방 끼리 직접 mesh 맞붙음
- **추가 코드**: `Elyqara.Dungeon` asmdef + 6 .cs (IRoom/RoomData/Room/DungeonManager/PlayerSpawnPositioner) + Editor 도구 3장 (`Phase6Setup` / `RoomSnapTool` / `DungeonGenerator`)
- **★ 사용자 톤 피드백 (★ 새 메모리 `feedback_no_pak_word.md`)**:
  - 사용자 직인: *"박는다는 표현은 왜그렇게 많이쓰는거야? 천박하긴한데 지적은 안했었거든? 근데 프로젝트 로그에까지 넣어버리는건 좀 그렇네"*
  - 향후 자제: 코드 주석 / Debug.Log / CLAUDE.md / 메모리 / 대화 모든 곳. 대체: *추가, 생성, 설정, 갱신, 처리, 등록, 정렬*. 예외 (단어 자체 박-): "그레이박스" / "박스" / "박치기"
  - cleanup 작업: CLAUDE.md / 모든 메모리 / 코드 주석 일괄 정리 — 130건 변환됨 (사용자 직인 인용 + 자제 대상 표시는 보존)
- **MCP schema 캐싱 함정 재학습**: 사용자 *"다 mcp로 진행 가능할거야"* 시점, Unity Editor 켜져있고 `unity-mcp Connected ✓`. 그러나 Claude session 의 deferred tool list 캐싱 = 이번 session 내내 schema 못 잡음. 정공 = Editor 스크립트 우회 (Phase6Setup / RoomSnapTool / DungeonGenerator). 같은 결과 + 컨텍스트 보존
- **NGO 2.x ConnectionApproval position 버그 회피**: WebSearch 결과 = 두 번째 클라이언트 (0,0,0) spawn 알려진 버그. 정공 패턴 = `PlayerSpawnPositioner : NetworkBehaviour` 의 server-side `OnNetworkSpawn` 에서 `transform.position` 직접 설정. server-auth NetworkTransform 가 다음 tick replicate
- **Door 위치 자동 정렬**: Phase6Setup 의 `ComputeDoorPositions` = 방 prefab 안 Ground (또는 가장 큰 Renderer) bounds 측정 → 동서남북 끝 중앙 + 중앙 SpawnPoint Y+1m
- **Snap 메뉴 (Cmd+Alt+S)**: 두 Door socket 선택 → 자동 align. 활성 = source (이동) / 다른 = anchor (고정). delta = anchor.world - source.world → sourceRoom.transform.position += delta
- **DungeonGenerator 7-room layout**: 십자 (StartRoom 동서남북 1개씩) + East 연쇄 2개 = 7개. 모두 StartRoom 복제 (1차 검증용 — 모양 다양성은 단계 12+ 콘텐츠 확장)
- **CLAUDE.md 분할**: 사용자 우려 *"파일 너무 커지면 너가 읽을때마다 시간이 걸리고 오류 발생할 확률"*. CLAUDE.md 498라인 → 핵심만 + `docs/` 분할 (sop / packages / decisions / elyqara-2d-ref). 매 turn 자동 로딩 부담 감소

## 2026-05-02 단계 7 통과 + NGO auto-populate 진단 학습 + 톤 룰 완화 (3차 직인)
- **단계 7 ✅** "적 잡으면 뭔가 떨어짐" — ItemDrop 파이프라인 + F키 픽업 + 그리드 인벤토리
- **사용자 직인**: *"좋아 모두 정상작동해"*
- **검증 시퀀스**: Wisp 처치 → 바닥 DroppedItem → I 키 인벤 → F 키 픽업 → 슬롯 표시
- **핵심 결정 (사용자 명확화)**: *"4명 분배는 없어 그냥 먹는사람인벤으로 들어가"* — 분배 메커니즘 X. 자유 경쟁 = 협동의 본질. CLAUDE.md 비전 #3 "같은 드랍을 같이 보고 분배" = *분배 = 자유 경쟁 결과* 명확화. 단계 8 = 코드 X (4명 검증만, 단계 11 합쳐질 가능성)
- **추가 코드**: `Elyqara.Items` asmdef + 11 .cs (IItem/ItemEffect/ItemData/ItemDatabase/ItemSlot/DropTableData/DroppedItem/ItemSpawner/Inventory/InventoryUI) + Player asmdef 2 새 (PlayerPickup/PlayerInventoryBinder) + 5 수정 (PlayerInput/Player.asmdef/Enemies.asmdef/EnemyData/EnemyController)
- **Editor 도구**: `Phase7Setup.cs` — 5 ItemData + ItemDatabase + WispDropTable + DroppedItem prefab + InventorySlot prefab + Wisp dropTable ref + Player 컴포넌트 + 씬 [InventoryCanvas] 자동 생성 (한 메뉴 클릭)
- **NGO 동기화 패턴**: `DroppedItem.NetworkVariable<int>` 인덱스 동기화. ItemDatabase Singleton 으로 클라가 같은 인덱스로 ItemData lookup. 단일 prefab + 인덱스로 모든 ItemData 처리 (prefab variant 안 만듦)
- **★ 진단 부족 지적 (NGO auto-populate 학습)**:
  - 사용자 직인: *"이거 큰문제야 니가 감을 못잡는거. 전체문서 다시읽고 너의 규칙도 다시 상기하고, 프로젝트 코드도 한줄도 빠짐없이 세팅도 다 읽고 인터넷 검색한뒤에 문제 해결해"*
  - Phase7Setup 후 클로드가 *"NetworkPrefabs 에 DroppedItem manual 추가 부탁"* 잘못된 안내 → 사용자 picker 창 (다른 화면) 보고 혼란
  - 진단 결과 = NGO 2.x `Assets/DefaultNetworkPrefabs.asset` auto-populate. 프로젝트 안 NetworkObject prefab 자동 추가. yaml read 검증 = Player + Wisp + DroppedItem GUID 자동 등록됨
  - **Hook**: 새 메모리 `feedback_ngo_auto_populate.md` — manual 등록 안내 X
- **★ 톤 룰 완화 (3차 직인)**:
  - 사용자 직인: *"박혀있을거좀 적당히 쓰는건 괜찮은데 ㅅㅂ 정신병자마냥 매순간순간 쓰니까 미친놈같잖아 적당히좀해"*
  - `feedback_no_pak_word.md` 갱신 — 완전 금지 X. 적당히 OK. 매 응답 / 매 문장 X. 빈도 룰 추가
- **1차 ItemData 풀 (5장 균일 가중치)**: Sword (Slash +12%) / Shield (Blunt +12%) / Amulet_Slash (+18%) / Amulet_Blunt (+18%) / Ring (Slash + Blunt 둘 다 +6%)
- **효과 적용 미루기**: `Inventory.GetTotalEffect` 메서드는 추가됨. BasicMeleeSkill 호출 시 곱 = 단계 9 본격 (데미지 파이프라인 시점)

## 2026-05-02 단계 9 통과 — 다운/부활 + 데미지 파이프라인 + Boss Room 패턴
- **단계 9 ✅** "한 명 죽어도 끝 아님" — Boss Room (Unity 공식 NGO sample) 패턴 차용
- **사용자 직인**: *"부활은 정상적으로 돼"* + *"응 좋아 정상적으로 표시되는것같아"* (인벤 표시 fix 후)
- **추가 코드**: `Elyqara.Game` asmdef 새 + 5 .cs (PlayerRevive/GameStateManager/GameOverUI/Phase9Setup) + 6 수정 (PlayerResources/Movement/SkillExecutor/Input/BasicMeleeSkill/Skills.asmdef)
- **핵심 패턴**:
  - 다운 ≠ Despawn — HP 0 시 `IsDown : NetworkVariable<bool>` true. 입력/물리 차단만. NetworkObject 유지
  - Revive = E키 2초 hold (Souls-like 톤). Owner ServerRpc → 호스트 거리 재검증 → ReviveServer(maxHp * 50%)
  - All-down 감지 = GameStateManager 호스트 polling 0.5초. 모든 PlayerResources.IsDown → IsGameOver = true
  - 데미지 파이프라인 = `damage = base * (1 + Inventory.GetTotalEffect(damageType))`. Elyqara 2D DamageCalculator 의 풀 RPG (회피/크리/방어/감소) = 단계 12+ 콘텐츠 확장
- **InventoryUI 표시 fix** (검증 도중 패치):
  - root cause = `GetComponentInChildren<Image>()` 가 slot 배경 Image 잡음 (Icon child 가 아님)
  - 정공 = `transform.Find("Icon")` 정확히 lookup + Icon null 시 placeholder 색깔 (회색 사각형) + count <= 1 시 itemName 폴백 표시
- **씬 placed NetworkObject 패턴**: `[GameStateManager]` 가 NetworkObject + GameStateManager 박힘. NGO Scene Management 활성 시 자동 spawn. NetworkPrefabsList 등록 X (씬 placed)
- **AttackProperty 위치 결정**: `Elyqara.Items` 안 `ItemEffectType` 유지 (YAGNI). 단계 12+ `Combat` asmdef 분리 가능

## 2026-05-02 첫 데모 스코프 락
- **사용자 직인**: *"캐릭터 1개 스테이지 3개 (마지막 스테이지는 보스 있게) 로비 스테이지 (멀티플레이어 로비 길드느낌으로) 적 종류 1개 보스 1개 이렇게 가면될것같아 UI도 조금 신경쓰고"*
- 한 런 흐름 = 로비 → 스테이지 1 → 2 → 3 (보스) → 클리어
- M12+ 백로그 = 캐릭터 추가 / 적 다양성 / 무기·방어구·소비 카테고리 / 희귀도 / 회복·시너지·메커니즘 효과 / 마스터리·메타 통화

## 2026-05-02 단계 10 (A+B+D) 코드 작성 + 사용자 1차 검증 OK
- **단계 10 코드 ✅** 사용자 1차 검증 = "되는것같아" (Lobby → Stage1 → Stage2 → Stage3_Boss → Victory 한 런 굴러감)
- **추가 코드**: `Phase10Setup.cs` Editor 도구 + 새 7 .cs (PlayerPersistence/StageTrigger/LobbyManager/VictoryUI/PlayerHUD/BossMarker) + 7 수정 (NetworkBootstrap/GameStateManager/GameOverUI/Game.asmdef/PlayerCamera/PlayerSpawnPositioner/EnemySpawner/InventoryUI)
- **씬 4개 분리**: Lobby (시작) / Stage1 / Stage2 / Stage3_Boss. Build Settings 등록
- **DDoL 패턴 채택** (NGO 2.x WebSearch 검증):
  - Player NetworkObject + vCam + GlobalManagers (GameStateManager) + UI Canvas (GameOver/Victory/HUD/Inventory) 모두 DDoL 마크
  - GameStateManager = placed 가 아닌 dynamic spawn (NetworkBootstrap 가 OnServerStarted 시 instantiate + Spawn(destroyWithScene=false)). late-join sync 안전
  - PlayerSpawnPositioner OnLoadEventCompleted 콜백 → DungeonManager.GetPlayerSpawnPosition 으로 씬 전환 후 위치 reset
  - 검증 근거: NGO 공식 docs *"If you're using scene switching, you can migrate the NetworkObject into the DDoL"*
- **사용자 manual 검증 시퀀스**:
  - Tools/Elyqara/Setup Phase 10-A (Scenes + DDoL)
  - Tools/Elyqara/Setup Phase 10-B (Boss)
  - Tools/Elyqara/Setup Phase 10-D (HUD)
  - Lobby Play → Host → Start Run → 한 런 검증
- **InventoryUI 누락 fix** (검증 도중 발견): Phase10Setup-A 가 Lobby 씬에 InventoryCanvas 자동 생성하도록 patch + InventoryUI.Awake DDoL 마크. Phase10Setup 모든 Ensure 메서드 idempotent 처리 (재실행 안전)
- **★ 사용자 미래 결정 (단계 11 통과 후 처리)**: *"따로 셋업하는 elyqara툴 말고 게임안에 다 넣어버리자"* — Phase{N}Setup Editor 메뉴 의존 줄이고 Canvas/Manager prefab 화로 통합 리팩터. 메모리 `feedback_integrate_setup_tools_into_game.md` 보관

## 2026-05-02 코드 점검 + 일괄 fix (HIGH 1 + MED 6 + LOW 4)
- **HIGH H2**: GameStateManager.OnNetworkSpawn — Singleton dup 시 NGO Spawned 객체 안전 정리 (`IsSpawned ? Despawn(true) : Destroy()` 분기)
- **MED M1**: GameStateManager.CheckAllDown — `validCount` 추적 → PlayerObject spawn 전 race 시 즉시 GameOver 회피
- **MED M2**: PlayerResources.ReviveServer — 절대값 → percent 전달. 캐릭터 max != 100 시 의도 깨짐 회피
- **MED M3**: StageTrigger — 다운 Player 차단 (사용자 직인 *"다운된 동료 끌고가는건 안되고"*)
- **★ 추가 명세**: GameStateManager.OnLoadEventCompleted — Stage 진입 시 모든 다운 Player 자동 100% HP 부활 (Lobby 제외). 사용자 직인 *"다음 스테이지로가면 동료가 부활하긴해"*
- **MED M4**: IDamageable.Faction (DamageFaction enum Player/Enemy) — BasicMeleeSkill / EnemyController 같은 Faction skip = FF off. 사용자 직인 *"FF off로 해줘 되는 스킬도 있을건데 스킬개발하면서 만들게"* (단계 12+ 백로그)
- **MED M5**: InventoryUI.Bind(null) → ClearAllSlots
- **LOW**: EnemyController.logStateTransitions=false 기본값 + EnemyController/PlayerResources OnHealthChanged `#if UNITY_EDITOR` 게이트 + Phase10Setup 빈 placeholder 메서드 제거

## 2026-05-02 마일스톤 재정의 — M13 (외형) 이 M11 (코어 검증) 보다 먼저
- **사용자 직인**: *"이제 그거 해야되거든 게임처럼 보이게. 길드 맵, 인게임 맵 (한 3개에서 5개정도 돌리면 될것같아) 보스룸 캐릭터/적애니메이션 이런거 등등 그래픽이 없으면 플레이하는 의미가 없으니께"*
- **재정의**: M10 → **M13 외형 정체성 (현재)** → M11 코어 검증 (외형 갖춘 데모로) → M12 콘텐츠
- **이유**: 그레이박스로는 친구 4명 검증 의미 약함. 외형 갖춘 후 검증이 정공
- **3-phase 분리** (사용자 plan):
  - Phase 13-A: 환경 mesh (코드 X, drag&drop). 던전 1방 (StartRoom prefab) → Lobby → 보스룸 → decoration
  - Phase 13-B: 모델 visual replace (Player/Wisp/Boss capsule 위에 모델만, 애니 X)
  - Phase 13-C: 애니 셋업 (Animator Controller + PlayerAnimator/EnemyAnimator)
- **★ Mesh 처리 방식 = 하이브리드** (Claude 추천 + 사용자 OK):
  - 외곽 (Floor/Wall/Ceiling) = ProBuilder 그레이박스 보존 + Material 교체. BoxCollider 안전 + Door socket/spawn point/dimension 보존
  - 안 (Decoration) = AI 3D mesh 자식 placement
  - 거부한 옵션: (a) 한 방 통째 AI mesh = MeshCollider 비-convex 함정 / (b) 완전 modular = AI 출력 톤 일관성 보장 X
- **AI 3D 툴 결정 = Phase 13-B/C 시점** (Phase 13-A 환경 = PBR 텍스처 위주라 AI 3D 툴 X)
- **사용자 직인**: *"다양한 ai툴을 사용해서 직접 만들어볼거야 ... 던전 방부터 시작하는게 좋을것같긴하다"*

## 2026-05-04 단계 13-1 통과 — 1층 (Stage1+2+3) Room-and-Corridor 자동 생성

- **사용자 직인**: *"좋다. 넌 최고야. 완벽하게 작동한은 것 같아. 머테리얼은 내가 파일구해와서 알려줄게."*
- **마일스톤 재정의** (사용자 직인 2026-05-04 *"스테이지 관련해서는 뒤집어도돼"*): 단계 6 결정 (직접 맵핑 7-room, 절차 생성 X) → 단계 13-1 (1층만 절차 생성, 옛 인프라 보존)
- **DanMachi 차용 락** (사용자 직인 2026-05-04):
  - ✅ 길드 hub 기능 / 던전 층별 정체성 / 길드 건물 시각 (다크 톤 필터링)
  - ❌ 세계관 (파밀리아/팔나/신의 은혜) — 백로그
  - 톤 = 다크소울/MH 무게감 유지. 라노벨톤 X. **타겟 = 서브컬처 수요**
- **게임 구조 락** (사용자 직인 2026-05-04): 게임 = 3층 / 층마다 = 스테이지 3개 / 각 층의 Stage3 = 보스 영역 / 층마다 테마 같음. 첫 데모 = 1층 (Stage1+2+3)
- **자동 생성 알고리즘**: Room-and-Corridor (TinyKeep Delaunay+MST+A* 단순화 = nearest-neighbor + L-shape). 시드 동기화 = NGO `NetworkVariable<int>` 호스트 권위. 시드 입자 = a (Stage 단위 매 런 새 시드). mesh = Unity primitive Cube + Material. Wall = cell 단위 분할 + 외부 인접 cell 이 floor 면 wall 생성 X (corridor 진입로 자동 뚫림 — 단계 13 fix 발견)
- **추가 코드**: `Elyqara.Dungeon` asmdef refs 에 `Unity.AI.Navigation` 추가. 새 클래스 2개 (`RuntimeDungeonGenerator : NetworkBehaviour` + `DungeonGenerationData : SO`). 옛 인프라 호환 변경 (`Room.cs` Setup() / `StageTrigger.cs` MonoBehaviour 변경 + Init() / `EnemySpawner.cs` Init() / `PlayerSpawnPositioner.cs` coroutine race 회피)
- **새 데이터/에셋** (Bash MCP 통한 자동 생성): `Floor1Data.asset` + RoomData 3장 (RuntimeSpawn/Generic/Exit) + Material 2장 (DungeonFloor/DungeonWall URP/Lit) + `WispBoss.prefab` (Wisp 복사 + BossMarker + scale 1.5x. HP 차별화 X — M11 후 EnemyData 차별화)
- **★★ MCP 자동화 학습 (큰 함정 우회)**:
  - Claude Code ToolSearch 에 MCP 도구 schema 안 보일 때 (옛 메모리 함정 = "새 session 까지 우회") → **Bash curl 직접 MCP HTTP 호출 = 우회 가능**. 단계 13-1 자동화 = 새 session 안 가고 현재 컨텍스트 끝까지 진행. CoplayDev 9.6.8 35+ 도구 모두 호출 가능
  - SerializedObject 가 NetworkBehaviour 상속 클래스의 ScriptableObject ref 적용 안 됨 발견 → **Reflection FieldInfo.SetValue 우회**가 정공. Component ref (NetworkObject 등) 는 SerializedObject OK
  - 상세 → `memory/feedback_bash_mcp_bypass.md` + `feedback_serializedobject_so_ref_fail.md`
- **신/계약 컨셉 백로그** (사용자 인용): *"신이랑 계약하는 컨셉도 마음에 들긴하는데 일단 개발하고 생각하자"* — M11 코어 검증 통과 후 결정. 비전 9개 1번 (다크소울/MH) 정합 (다크소울 = 신/계약 모티프 강함)
- **사용자 manual 작업 대기**: Material 파일 별도 제공 (사용자 = *"머테리얼은 내가 파일구해와서 알려줄게"*). 텍스처 입힌 Material 받으면 DungeonFloor.mat / DungeonWall.mat 교체
- **다음 시작점 (선택지)**:
  - a. **M11 코어 검증** (친구 4명 플레이테스트) — 외형 부족하지만 코어 검증 가능
  - b. **단계 13-2 (캐릭터 외형)** — Kiyan 모델/visual replace
  - c. **단계 13-1 fine-tune** — 미로 분기 (recursive backtracker), 시각 톤 강화 (사용자 Material 받은 후)

## 2026-05-05 게임플레이 컨셉 락 — 세계관 + 무기 폐기 + 스킬 풀 + 신앙심 매트릭스

- **세계관 락**: Elyqara = 고대 던전 위 대도시 (던만추 오라리오 패턴). 다양 종족 / **인간 메인** (1차 모든 캐릭 인간). 모험가 = 마물 잡고 생계 + **신 계약 부활** = 직업 본질. 던전 = 생활 위해 아래로. 매 런 = 9 스테이지 = 1 사이클 (로그라이크). Elyqara 2D 채용 = **캐릭터만**
- **비전 #4 변경**: 무기 카테고리 시스템 폐기 (사용자 직인 *"무기 카테고리는 없어도 돼. 시스템을 단순하게 하자"*). 캐릭별 visual 고정만. 비전 #6 (속성 1차 = Slash/Blunt) 그대로 — 속성 = 스킬 단위
- **빌드 공식 락**: 한 런 = 캐릭 × 스킬 (4 슬롯) × 신. 메타 = 캐릭별 숙련도 + 캐릭×신 신앙심 매트릭스
- **스킬 시스템**: 4 슬롯 × 캐릭별 풀. 캐릭터 숙련도 따라 풀 해금. 플레이어가 슬롯 끼울 스킬 선택 = 플레이스타일
- **신 시스템**:
  - **M11 = 신 1명 자동 (선택 X)**, 이름/lore = placeholder. 코어 = *"Kiyan 은 신과 계약했다"* 정도 보존
  - **M12+ 본격** = 신 풀 5~7명 (Hades 패턴 + 신앙심 매트릭스). 신앙심 = 캐릭×신 별로 따로 (영구 진행)
- **DanMachi 차용 추가 갱신**: 파밀리아 = 폐기. 모험가 레벨 / 신 부활 / 신 효과 = M12+ 백로그. 신 계약 부활 lore 만 = 코어 (모든 모험가 본질)
- **사용자 직인** (2026-05-05):
  - *"무기 카테고리는 없어도 돼. 시스템을 단순하게 하자"*
  - *"인간메인으로 가자 우리도"*
  - *"elyqara2d 설정은 캐릭터외에 하나도 채용하지 않아"*
  - *"캐릭터는 생활기반 모험가고 특정 신과 계약해서 부활이라는 능력을 얻어"*
  - *"신은 일단 초기단기에서는 한명만 하고 신 숙련도 (신앙심?이런거)도 캐릭터별로 만들자"*
  - *"숙련도는 캐릭터별로 하고 신은 풀이 같아"*
- **상세** → `memory/gameplay_concept_lock_20260505.md`
- **남은 결정** (M12+ 진입 시): 신 5~7명 디자인 / 신앙심 효과 강화 방식 / 캐릭터 숙련도 페널티 / 신 부활 페널티 / 캐릭터별 종족 다양화

## 2026-05-05 추가 — Kiyan 스킬 키트 코드 작성 + Karpathy 글로벌 가이드 적용

### Kiyan 스킬 키트 코드 (검증 대기)
- 옛 직인 *"그냥 캐릭터 컨셉만 박아줘"* 기반 코드 7 파일 작성 (Skills 5 + PlayerResources/BasicMelee 2)
- 사용자 의도 = 설정 우선이라 작업 중단 → SO/prefab 통합 미완
- 컴파일 OK. 다음 session = 검증 (SO 생성 + Kiyan.asset 갱신 + Player.prefab Passive) 또는 revert 결정
- 상세 → `memory/kiyan_skill_kit_code_pending_20260505.md`

### Karpathy 4 원칙 글로벌 적용
- `~/.claude/CLAUDE.md` 새 생성 (Karpathy LLM coding pitfall 4 원칙: Think Before Coding / Simplicity First / Surgical Changes / Goal-Driven Execution)
- 출처: `https://github.com/forrestchang/andrej-karpathy-skills`
- 모든 프로젝트 자동 prepend. 우리 절대 원칙 10 + 정합 (보완)
- 새 session 부터 효과

## 2026-05-20 비주얼 톤 갱신 — 어두운 일본 판타지 (던만추式)

- **사용자 직인**: *"게임의 톤을 조금 수정해줘. 던만추 느낌의 일본식 서양 판타지 느낌으로 가고싶어"* + 선택 = "중간 — 어두운 일본 판타지"
- **변경**: 비주얼 톤 락 = "다크판타지 × 일본 서브컬쳐 (베르세르크 / 그랑블루 / 다크소울 일본판 / Octopath)" → **"어두운 일본 판타지 (던만추式 일본 서양 판타지 + Octopath / 다크소울 일본판)"**
- 일본 애니 작화 채택. 베르세르크式 그로테스크 강도 완화. 다크판타지 분위기(어두운 던전·진중함)는 유지. 라노벨 전면 cute 톤은 여전히 X
- **전투 톤 = 변경 X** — 비전 #1 (다크소울/MH 무게감) 그대로. 톤 수정 = 비주얼 한정
- 던만추 세계관/구조(오라리오식 던전 도시)는 이미 차용 락 — 비주얼도 던만추로 맞춤 = 정합
