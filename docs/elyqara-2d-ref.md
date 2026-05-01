# Elyqara (2D) — read-only 레퍼런스

## 관계

`/Users/jeong-yunsu/Elyqara/` 는 1주짜리 바이브코딩 프로토타입. 핵심 로직 가치 있다고 본인 판단 → 같은 IP/이름 사용. **read-only 레퍼런스 — 직접 포팅 X. 패턴 참조 + 데이터 추출만.**

## KEEP

- 판타지 이세계 세계관 + Elyqara 이름/IP
- 검사 캐릭터 컨셉 (검방패로 구체화)
- Forest 바이옴 톤 (3D 재해석은 백로그)
- 던전 크롤러 + 로그라이크 구조

## CUT

- v4 88퍽 / 15세트 / 16 Legendary / 보석 / 5트리×5티어 / 8종 특수방 / 시너지 → 제거 (복잡도 폭발 원인)
- 4 캐릭터 → 1 캐릭터
- 4 스테이지 → 1 던전

## NEW (3D 차기작 only)

- 3D 3인칭 어깨 너머
- 자율적 역할구조 (트리니티 그림은 살아있되 빌드로 형성)
- 호스트 권한 멀티 (NGO + Steam)
- 무기 캐릭터별 고정 + RPG 아이템 빌드 (BG3/바간테식)
- 다운→부활
- 다크소울/MH급 무거운 전투

## 가치 있는 패턴 (3D 포팅 시 참조 위치)

| 패턴 | Elyqara 2D 위치 | 차용 단계 |
|---|---|---|
| 데미지 파이프라인 | `DamageCalculator.cs` / `RuntimeStatHolder.cs` | 단계 9 다운/부활 시 본격 |
| 적 AI FSM | (Elyqara 2D enemy 위치) | 단계 4 — 이미 차용 (단 Souls-like 표준 패턴으로 갈아엎힘) |
| 그리드 인벤토리 패턴 | `Assets/_Project/Scripts/UI/Inventory/` (`GridDropZoneUI` 등) + Items 시스템 | 단계 7 차용 |
| 그리드 던전 파이프라인 | `Assets/_Project/Scripts/Systems/Stage/GridMap/` (GridMapGenerator / GridDungeonBuilder / GridRoomManager) | 단계 6 = 차용 X (직접 맵핑으로 결정) |
| 세이브 시스템 | (Elyqara 2D save 위치) | 백로그 |

## 단계 7 진입 시 read-only 분석 위치

- `/Users/jeong-yunsu/Elyqara/Assets/_Project/Scripts/UI/Inventory/` — 그리드 인벤토리 UI 패턴
- `/Users/jeong-yunsu/Elyqara/Assets/_Project/Scripts/Systems/Items/` — 아이템 시스템 (위치 미확정 — Glob 으로 확인 필요)
- `/Users/jeong-yunsu/Elyqara/Assets/_Project/Scripts/Data/ItemData.cs` 또는 비슷 — ItemData ScriptableObject 패턴

## 커리어 컨텍스트 (사이드 강등)

- **메인 트랙(비자→MBA→고도인재)이 우선**, 게임은 사이드 자산
- **단기**: 데모 출시 → 일본 게임업계 포트폴리오
- **중기**: Day 1부터 유튜브/X 공개 → 콘텐츠 자산
- **장기**: SAO급 3D VR RPG 궁극 목표의 첫 마일스톤

**Day 1 콘텐츠 전략**: 매주 dev log 업로드. 회색 캐릭 시기 = "정체성 없이 게임 만드는 과정" 자체가 콘텐츠.
