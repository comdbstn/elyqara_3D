---
name: add-item
description: 차기작에 새 아이템 추가. ItemData SO 1장 + ItemEffect 배열 + ItemDatabase 등록. 1차 효과 = Slash/Blunt 데미지 % 만.
argument-hint: "[item-name] [slot: weapon|armor|accessory]"
allowed-tools: Read, Grep, Glob, Bash, Write, Edit
---

# 새 아이템 추가 SOP

`ItemData : ScriptableObject` + ItemEffect[] 배열. 새 아이템 = SO 1장 + ItemDatabase.items 배열 등록. 코드 0줄.

## Input

`$ARGUMENTS` = `[item-name] [slot]` (예: `LegendarySword weapon`, `IronShield armor`, `BlessedRing accessory`)

## Step 1 — 옛 패턴 확인 (★ 환상 방지)

- `Assets/_Project/Scripts/Items/ItemData.cs` — SO 필드 (Identity / Grid / Effects[])
- `Assets/_Project/Scripts/Items/ItemEffect.cs` — `ItemEffectType` enum (SlashDamageBonus / BluntDamageBonus — 1차)
- `Assets/_Project/Scripts/Items/IItem.cs` — 인터페이스
- `Assets/_Project/Scripts/Items/ItemDatabase.cs` — Resources/ Singleton lookup
- `Assets/_Project/Scripts/Items/DropTableData.cs` — 가중치 Roll
- `Assets/_Project/Data/Items/` — 옛 5종 (Sword_Common, Shield_Common, Amulet_Slash, Amulet_Blunt, Ring_Generic)

> 못 찾는 파일 즉시 멈춤. 추측 X.

## Step 2 — 1차 효과 범위 확인 (★ 비전 락)

비전 락 (CLAUDE.md):
- 1차 효과 = **Slash 데미지 +X% / Blunt 데미지 +X% 만**
- 회복 % / 시너지 / 메커니즘 변화 = **백로그**. 1차 마일스톤에 추가 X
- 원소 (Fire/Cold/Lightning) / Pierce = **백로그**

> 사용자가 백로그 효과 요청하면 — *"이건 백로그 효과인데, M11 코어 검증 후로 미룰까?"* 확인. 1차에 끌어들이지 말 것.

## Step 3 — 데이터 추가

`Assets/_Project/Data/Items/[item-name].asset` 생성.

ItemData SO 필드:
- `itemName` — 표시 이름
- `description` — 한 줄 설명 (선택)
- `icon` — Sprite (M11 단순 회색 박스 OK)
- `gridSize` — Vector2Int. 1x1 기본. 2x1 같은 큰 아이템은 M12+
- `effects[]` — ItemEffect 배열. 1차 = SlashDamageBonus / BluntDamageBonus 만

> 값은 옛 아이템 변주. Common·Rare 단계 정함 X — 사용자에게 명시 값 받기.

## Step 4 — ItemDatabase 등록

`Assets/_Project/Data/ItemDatabase.asset` (또는 Resources/) 의 `items[]` 배열에 새 SO 추가. Spawn 시 네트워크 인덱스 lookup 동기화용.

> 등록 안 하면 `DroppedItem` 가 네트워크 동기화 못 함. **반드시 등록**.

## Step 5 — DropTable 연결 (선택)

새 아이템이 특정 적·방에서 떨어지게 하려면 — `DropTableData.asset` 에 가중치로 추가. `EnemyData.dropTable` 가 그 테이블 참조.

## Step 6 — 검증

- [ ] ItemData SO 1장 생성
- [ ] effects[] = 1차 효과 (Slash/Blunt 데미지 %)만
- [ ] gridSize = 1x1 (M11) 또는 비전 락 안 변동
- [ ] ItemDatabase.items 등록
- [ ] DropTable 가중치 — 떨어뜨릴 곳 있으면
- [ ] 4×6 그리드 인벤토리에서 픽업·드래그 정상 작동

## 절대 사수

- **★ 비전 락** — 1차 효과 범위 (Slash/Blunt %) 안 깨기. 백로그 효과 1차에 끌어들이지 말 것
- **★ 옛 코드 우선** — ItemEffect / ItemDatabase / DropTableData 패턴 직접 Read 후 진행
- **★ 환상 금지** — 옛 5종 (Sword_Common 등) 데이터 직접 Read 후 변주. 임의 값 X
- **단순화** — 새 ItemEffectType 자작 X. 1차 = SlashDamageBonus / BluntDamageBonus
- **메모리 즉시 갱신**
