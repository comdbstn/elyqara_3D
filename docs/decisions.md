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
