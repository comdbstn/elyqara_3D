# 패키지 + 시스템 도구

## manifest.json 패키지 표

| 패키지 | 버전 | 역할 |
|---|---|---|
| `com.coplaydev.unity-mcp` | git/main | Claude ↔ Unity MCP 브릿지. Editor 안에서 도구 호출 받음 |
| `com.unity.netcode.gameobjects` | 2.4.4 | NGO 2.x — 호스트 권한 네트워킹. NGO 1.x는 6000.3 deprecated |
| `com.unity.probuilder` | 6.0.9 | ProBuilder — Unity 안 그레이박스/레벨 모델링. 단계 5+ 던전 방 prefab. **버전 결정 = Package Manager UI 통한 설치만 (Unity 공식 호환 자동 결정. manifest 임의 박기 X — 추측 금지)** |
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

## 아직 추가 안 함 (단계별 추가)

- Animation Rigging (단계 2 검방패 IK — 백로그)
- TextMeshPro — 6000.3에서 빌트인 모듈로 흡수 (별도 추가 불필요)

## 시스템 도구 상태 (2026-05-01 검증)

| 도구 | 상태 | 경로 |
|---|---|---|
| Homebrew | 5.1.0 ✅ | `/opt/homebrew/bin/brew` |
| Python 3 | 3.12.13 ✅ | `/opt/homebrew/bin/python3` (symlink → 3.12.13) |
| uv | 0.11.8 ✅ | `/opt/homebrew/bin/uv` |
| Node | 24.8.0 ✅ | (gamelovers MCP 호환용. CoplayDev 활성화 시 의존성 X) |
| Unity | 6000.3.7f1 ✅ | `/Applications/Unity/Hub/Editor/6000.3.7f1` |
| Claude Code | 활성 ✅ | |
| /etc/paths.d/homebrew | ✅ | `/opt/homebrew/bin` GUI 앱 PATH 자동 포함 |

## MCP 검증

`claude mcp list` 결과 = `unity-mcp: http://127.0.0.1:8080/mcp (HTTP) - ✓ Connected`. Unity 켜져있는 동안만 작동 (서버가 Unity 안에서 돌아감). 첫 도구 호출 전 항상 검증 권장.

★ 함정: Unity Editor 재시작 또는 *나중에 켜기* 시 Claude session 의 deferred tool list 캐싱 = schema 못 잡음. 새 session 까지 우회 = `Edit` + `Bash` + Editor 스크립트.

## Steam P2P 트랜스포트

보류. 단계 1 = NGO 기본 UTP(Unity Transport)로 LAN 테스트. 커뮤니티 Facepunch 트랜스포트는 컴파일 깨짐 확인 (FacepunchTransport.cs:288 CS1028) → 단계 1 검증 후 별도 솔루션.
