# HANDOFF — 2026-05-01

> 한 시간 자리비운 사이에 끝낸 것 + 너가 돌아와서 해야 하는 것.
> Unity 안 클릭만 하면 끝남. 그 이후 단계 1 시작 가능.

---

## ✅ 자동으로 끝낸 것

### 1. 시스템 의존성 설치
```
Homebrew 5.1.0          ✅ 이미 있음
Python 3.12.13          ✅ 새로 설치 (/opt/homebrew/bin/python3 → 3.12)
uv 0.11.8               ✅ 새로 설치 (/opt/homebrew/bin/uv)
Node 24.8.0             ✅ 이미 있음
```
- macOS GUI 앱이 `/opt/homebrew/bin` 인식하도록 `/etc/paths.d/homebrew` 박혀있는 거 확인 → Unity 재시작하면 알아서 잡힘

### 2. 프로젝트 설정 파일

| 파일 | 역할 | 상태 |
|---|---|---|
| `.claude/settings.json` | Claude 권한 (allow/ask/deny) + MCP 도구명 + Logs deny 제거 (디버그 차단 방지) + brew/git/python 도구 분류 | ✅ 갱신 |
| `.gitignore` | Unity 6 + Claude 캐시 + macOS + Steam 로그. `*.meta` 절대 제외 안 함 | ✅ |
| `.gitattributes` | Unity Git LFS 권장 (3D/텍스처/오디오 자동 LFS), 줄바꿈 LF, .meta 는 LFS 제외 | ✅ 신규 |
| `.editorconfig` | C# 코드 스타일, 네이밍 (`_camelCase` private 필드), Unity 자산은 손대지 않음 | ✅ 신규 |
| `Packages/manifest.json` | NGO 2.4.4, Cinemachine 3.1.6, CoplayDev MCP 박힘. Facepunch 트랜스포트 제거 (컴파일 깨짐) | ✅ |
| `Assets/_Project/{Scripts,Data,Prefabs,Scenes,Settings,Art,Audio}/` | 폴더 스켈레톤. `.gitkeep` 있어서 git 시 비어도 추적 | ✅ |

### 3. CLAUDE.md 갱신
- "현재 상태" 섹션: 패키지 표 + 시스템 도구 검증표
- "패키지 추가 SOP" 섹션 신설 (오늘 Facepunch 폭발에서 학습한 규칙: 한 번에 하나씩, GitHub 검증 후, 컴파일 확인)
- "리서치 ≠ 검증" 섹션 신설

### 4. 글로벌 메모리 갱신
8개 파일:
- `MEMORY.md` (인덱스)
- `mcp_server_location.md` — Planet_Destroyer 의존 함정
- `steam_appid_480.md` — dev AppID 정책
- `unity_mcp_refs.md` — 핵심 URL 모음
- `feedback_thorough_yagni.md` — 광범위 위임 시 행동 패턴
- `feedback_meta_files.md` — `*.meta` 보호
- `feedback_ngo_v2.md` — NGO 2.x 함정
- `feedback_verify_community_packages.md` — 커뮤니티 패키지 검증 필수 (Facepunch 폭발에서 학습)

---

## 👤 너가 돌아와서 해야 하는 것 (5분 내 끝)

### Step A. Unity 완전 재시작
이미 켜져있는 Unity 종료 후 다시 켜기. **터미널에서 `open -a Unity` 권장** (Finder 에서 띄우면 PATH 안 잡히는 함정 있음). 단, 이미 `/etc/paths.d/homebrew` 박혀있어서 Finder도 작동할 가능성 큼 — 안 되면 터미널 방식.

### Step B. MCP for Unity Setup 창에서 Refresh
1. Unity 메뉴: `Window > MCP for Unity`
2. **Refresh** 버튼 클릭
3. Python, UV 둘 다 **초록 점** 으로 바뀌는지 확인
4. 초록 안 되면 ↓ "초록 안 될 때" 섹션 참조

### Step C. Claude Code 와 연결
같은 창에서:
1. **Claude Code** 선택 (목록에 있을 것)
2. **Configure** 버튼 클릭 → CoplayDev 가 자동으로 Claude Code MCP config 갱신
3. 창 하단 "Connected ✓" 초록 점 확인
4. **Done** 클릭

### Step D. 옛 MCP 정리 (터미널에서)
```bash
# 옛 gamelovers MCP (Planet_Destroyer 경로) 제거
claude mcp remove unity-mcp

# 검증 — CoplayDev 만 남아있어야 함
claude mcp list
```

### Step E. Claude Code 세션 재시작
- 이 채팅 종료
- 새 Claude Code 세션 시작 (터미널에서 `claude` 또는 IDE 에서 시작)
- 새 세션에서 "단계 1 시작" 이라고 말하면 → SteamLobbyService 빼고 NetworkBootstrap + Player 프리팹 + 캡슐 두 개 동기화 (UTP LAN 테스트) 진행

---

## 🔧 트러블슈팅

### Refresh 눌러도 빨간 점 (Python / UV not found)
1. 터미널에서 검증:
   ```bash
   which python3        # → /opt/homebrew/bin/python3 여야 함
   python3 --version    # → Python 3.12.13
   which uv             # → /opt/homebrew/bin/uv
   uv --version         # → uv 0.11.8
   ```
   다 나오면 → Unity가 PATH 못 잡은 것. Unity 종료 후 터미널에서 `open -a Unity` 로 재실행.

2. 위가 다 나오는데도 Unity 가 못 찾으면:
   ```bash
   launchctl setenv PATH "$PATH"
   ```
   실행 후 Unity 다시 재실행 (Finder/Dock 에서 켜도 됨).

### Configure 눌러도 Claude Code 에 안 잡힘
```bash
claude mcp list
```
- `unityMCP` 또는 `unity-mcp` 가 **CoplayDev 경로** (`Library/PackageCache/com.coplaydev.unity-mcp@*` 또는 비슷) 로 등록됐는지 확인
- 안 잡혔으면 Unity Setup 창에서 다시 Configure
- 그래도 안 잡히면:
  ```bash
  claude mcp add unity-mcp -- uv run --directory /Users/jeong-yunsu/Elyqara_3D/Library/PackageCache/com.coplaydev.unity-mcp*/Server~ python -m server
  ```
  (정확한 경로는 `ls Library/PackageCache/ | grep coplaydev` 로 확인)

### 컴파일 에러 다시 나면
```bash
tail -60 ~/Library/Logs/Unity/Editor.log
```
- 빨간 줄 (error) 캡처해서 새 세션 클로드에게 보내기
- MCP 깨졌어도 이 명령은 작동 (프로젝트 외부 로그)

---

## 🎯 다음에 들어갈 작업: 단계 1

**명제**: "둘이 같이 움직임" (캡슐 2명 NGO LAN 동기화)

세부:
- `Assets/_Project/Scripts/Networking/Elyqara.Networking.asmdef`
- `NetworkBootstrap.cs` — NetworkManager + UTP transport. host/client UI 버튼
- `Player.prefab` — Capsule + NetworkObject + NetworkTransform + 간단 입력 (WASD)
- 검증: 같은 머신에서 빌드 + 에디터, 또는 두 머신 LAN. 둘 다 움직이고 동기화

**Steam P2P 트랜스포트는 단계 1 검증 통과 후 별도 단계**:
- Facepunch 트랜스포트 fork 찾거나
- NetworkTransport 직접 작성 (~200줄)
- 그동안 네트워킹 개발은 UTP localhost 로 충분

---

마지막으로: 한 번에 다 깰 위험 줄이려고 패키지 1개씩 추가 + 컴파일 검증 SOP CLAUDE.md 에 박았음. 단계 1 들어갈 때 ProBuilder/Animation Rigging 같은 것도 1개씩 추가해서 검증. 더는 4개 동시 추가 같은 짓 X.
