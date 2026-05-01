# SOP — 패키지 추가 / 환경 fix / Unity MCP 워크플로우

> CLAUDE.md 의 절대 원칙 #1 (옛 코드 우선 확인) + #5 (환상 금지) + #10 (정공법 우선) 의 *도구 호출 순서* 적용편. 단계마다 빼먹지 말 것.

---

## 패키지 추가 SOP (2026-05-01 Facepunch + 2026-05-02 ProBuilder 6.0.4 폭발에서 학습)

> 리서치 결과 ≠ 검증된 사실. 적용 전 컴파일 1회 필수.
> ★ **버전 = 검증된 source 직접 인용 만**. 추측 금지 (절대 원칙 #5)

1. **한 번에 1개씩 추가** — 여러 패키지 동시 추가 절대 금지. 깬 게 어느 건지 격리 못 함
2. **★ Unity 공식 패키지** (`com.unity.*`) — Claude 가 manifest 직접 수정 X. 사용자에게 *Window > Package Manager > Unity Registry > [패키지명] > Install* UI 통한 설치 부탁. Unity 가 호환 권장 버전 자동 결정 + manifest.json 자동 update. **예외**: 메모리/CLAUDE.md 에 검증된 버전 있는 경우 (예: NGO 2.4.4, Cinemachine 3.1.6) 만 manifest 직접 수정 OK
3. **★ 커뮤니티 / git URL 패키지** — GitHub 최근 커밋 + open issues 확인 후 manifest 추가. 단 버전 = README/release page/메모리 검증된 값 직접 인용
4. 추가 → Unity 재컴파일 → `~/Library/Logs/Unity/Editor.log` (또는 MCP `read_console`) 으로 컴파일 에러 0건 확인
5. 에러 있으면 **즉시 manifest 에서 제거** (해결 시도 X — 다른 패키지 추가 막힘) + **`Library/PackageCache/[패키지명]@*` 폴더 삭제** (캐시 손상 정공) + 사용자에게 Package Manager 통한 재설치 부탁
6. 클린 확인 후 다음 패키지

---

## 환경 fix — PackageCache 손상 (캐시 깨짐 패턴)

> 패키지 추가 fatal 시 manifest 변경만으로는 *깨진 캐시* 가 남음. 다음 설치 시도도 같은 fatal 재발

1. Unity Editor 완전 종료
2. Finder 또는 터미널에서 `Library/PackageCache/com.unity.[패키지명]@*` 폴더 삭제 (Unity 가 자동 재생성하는 캐시. Asset 손상 X)
3. **Library 전체 삭제** 까지 가면 후 reimport 시간 큼 — 표적 정리 우선
4. Unity Editor 재시작 → Package Manager 가 처음부터 다시 다운로드 → 클린 설치

---

## 리서치 ≠ 검증

- 리서치 에이전트가 가져온 정보는 **가설**. 적용 전 한 번 더 확인.
- "워킹 한다" / "fine for 2026" 같은 표현은 작성자 환경 기준. 우리 Unity 6000.3.7f1 + macOS 24.6 에서 검증되지 않음
- MCP / 에디터가 깨지면 `~/Library/Logs/Unity/Editor.log` 직접 읽기 (deny rule 우회 가능 — 프로젝트 외부)

---

## Unity MCP + Claude Code 워크플로우 (커뮤니티 검증 패턴)

```
1. SEARCH (절대 원칙 #1)
   → Grep / Glob / code-pattern-checker 에이전트로 옛 패턴 조사
   → 못 찾으면 사용자에게 묻기 (추측 금지)

2. PLAN
   → 데이터로 끝나나? (ScriptableObject 한 장)
   → 새 코드 필요하면 어느 매니저/인터페이스 안인가?

3. EDIT
   → 새 .cs: Write 도구
   → 기존 .cs 수정: Edit 도구 (GUID 안전)
   → 씬/프리팹 변경: mcp__unity-mcp__* (또는 Editor 스크립트 우회)

4. RECOMPILE
   → mcp__unity-mcp__recompile_scripts (또는 Unity Editor focus 자동)

5. L1 — 컴파일 검증
   → mcp__unity-mcp__get_console_logs (errors only)
   → 에러 있으면 → 3번으로 복귀

6. L2 — 런타임 검증 (필요할 때만)
   → mcp__unity-mcp__execute_menu_item "Edit/Play"
   → get_console_logs (런타임 예외)
   → execute_menu_item "Edit/Play" 다시 (정지)

7. SAVE (씬/프리팹 변경 시만)
   → mcp__unity-mcp__save_scene

8. 메모리 갱신 (절대 원칙 #6)
   → 시스템 인덱스에 한 줄 추가
   → CLAUDE.md / 글로벌 메모리 둘 다
```

### 도구 사용 규칙 (2026-05-01 권한 게이트 제거 후)

- **거의 모든 도구 allow** — 사용자 결정 (review 능력 부재로 ask 마찰 무의미). Claude self-discipline 가중 (10원칙 + SOP self-check)
- **Destructive 만 deny**: rm -rf / git reset --hard / git push --force / git branch -D 등 후처리 불가능한 것만
- **Library/Temp/Logs/Build/obj/UserSettings 읽기 deny**: 토큰 낭비 방지. MCP 깨졌을 때만 `~/Library/Logs/Unity/Editor.log` 직접 읽기 (프로젝트 외부)
- **MCP 도구 함정**: Unity Editor 재시작 후 Claude session 의 deferred tool list 갱신 X (캐시). 새 세션 시작 시까지 MCP 도구 schema 못 잡음. 이때 `Edit` + `Bash` + Editor 스크립트 우회 (단계 6 = `Phase6Setup` / `RoomSnapTool` / `DungeonGenerator` 패턴)

---

## 알려진 함정

- `*.meta` 파일 절대 git에서 제거 금지 — GUID 깨지면 프리팹 사일런트 파괴
- NGO 1.x 튜토리얼 따라가지 말 것 — 6000.3에서 deprecated. 2.x API 다름
- Cinemachine 2.x 코드 (`CinemachineVirtualCamera`) 쓰지 말 것 — 3.x는 `CinemachineCamera`
- Unity 가 컴파일 중일 때 `save_scene` 호출 금지 — race condition. 항상 recompile_scripts → console 클린 확인 → save 순서
- macOS Finder에서 Unity 띄우면 PATH 안 넘어감 — Terminal에서 `open -a Unity` 권장
- NGO UnityTransport 의 PlayMode Stop 시 7777 UDP socket 좀비 — `NetworkManager.Shutdown(true)` 도 native release 못 함. NetworkBootstrap PlayMode-stop hook 있어도 한계. **Unity Editor 재시작이 환경 정공**
- MPPM Player 2 코드 변경 자동 reload X — 변경 후 OFF/ON 토글 필수
- NGO 2.x ConnectionApproval 의 Position 파라미터 = 두 번째 클라이언트 (0,0,0) spawn 알려진 버그. 정공 = `NetworkBehaviour` 의 server-side `OnNetworkSpawn` 에서 `transform.position` 직접 설정 (예: `PlayerSpawnPositioner`)
- ProBuilder Shape Editor `SerializedObject Disposed` 경고 = 패키지 자체 race condition. 우리 코드 X. 무시
- TextMeshPro `Can't Generate Mesh, No Font Asset has been assigned` = TMP Essentials 미import. 단계 진행 막지 X. 정리 = Window > TextMeshPro > Import TMP Essentials
