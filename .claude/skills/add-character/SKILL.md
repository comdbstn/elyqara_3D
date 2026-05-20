---
name: add-character
description: 차기작에 새 캐릭터 추가. CharacterData SO 1장 + 4슬롯 스킬 wire-up. M11 = Kiyan 1캐릭. M12+ 부터 추가 캐릭.
argument-hint: "[character-name]"
allowed-tools: Read, Grep, Glob, Bash, Write, Edit
---

# 새 캐릭터 추가 SOP

`CharacterData : ScriptableObject` + 4슬롯 SkillData 참조. 새 캐릭 = SO 1장 + 스킬 4개 (있거나 새로 만들거나) + Player.prefab 의 PlayerCharacterBinder.character 한 줄 변경.

## ⚠️ M11 단순화 — 이 SOP 실행 전 확인

비전 락 (CLAUDE.md):
- **M11 = Kiyan 1캐릭만**. 새 캐릭은 M12+ (코어 검증 통과 후)
- M11 코어 검증 통과 전에 새 캐릭 추가하지 말 것

> 사용자가 M11 전에 새 캐릭 요청하면 — *"M11 코어 검증 후로 미루는 게 비전 락. 진행할까?"* 확인.

## Input

`$ARGUMENTS` = `[character-name]` (예: `Mando`, `Haeroi`, `Silvia`)

## Step 1 — 옛 패턴 확인 (★ 환상 방지)

- `Assets/_Project/Scripts/Characters/CharacterData.cs` — SO 필드 (Identity / Resources / 4 SkillData 슬롯)
- `Assets/_Project/Data/Characters/Kiyan.asset` — 첫 캐릭 참조
- `Assets/_Project/Scripts/Player/PlayerCharacterBinder.cs` — CharacterData ref
- `Assets/_Project/Scripts/Player/PlayerSkillExecutor.cs` — 4슬롯 → ISkill 실행

> 못 찾는 파일 즉시 멈춤.

## Step 2 — 무기·역할 정의 (★ 비전 락)

비전 락:
- 무기는 캐릭별 **visual** 고정 (Kiyan = 검방패)
- 무기 카테고리 시스템 X (2026-05-05 단순화)
- 자율적 역할구조 — 빌드로 역할 형성. 캐릭이 역할 강제 X
- 속성 = 스킬 단위. 캐릭 자체엔 속성 X

> 새 캐릭 정의 시 사용자에게 다음 확인:
> - visual 무기 (예: 양손검 / 활 / 마법봉)
> - 4슬롯 스킬 4개 (기존 풀 재사용? 새로?)
> - 자율 역할 분기 (어떤 빌드로 어떤 역할?)

## Step 3 — 스킬 4개 확보

다음 중 하나:
- **기존 스킬 재사용** — Kiyan 의 BasicMelee·ShieldBash 등 그대로 사용 가능
- **새 스킬 추가** — `/add-skill` SOP 실행하여 새 스킬 1개씩. 4개 다 새로면 4번 실행

> 스킬 자작 시 *환상 금지* — 옛 SkillData 패턴 직접 Read 후 변주.

## Step 4 — CharacterData SO 생성

`Assets/_Project/Data/Characters/[character-name].asset` 생성.

필드:
- `characterName` — 표시 이름
- `maxHealth` — Kiyan 변주
- `maxStamina`, `staminaRegenPerSec`
- `primarySkill` / `secondarySkill` / `qSkill` / `dodgeSkill` — Step 3 의 SkillData 4개 wire-up

> 수치는 *Kiyan 변주*. 사용자 명시 값 받기.

## Step 5 — Player.prefab wire-up

`Player.prefab` 의 `PlayerCharacterBinder.character` 필드를 새 CharacterData 로 교체.

> 멀티 환경에서 *호스트가 캐릭 선택*하면 클라가 동기화. 캐릭 선택 UI 는 M12+ (캐릭 1명 시기엔 prefab 직접 교체로 충분).

## Step 6 — 검증

- [ ] CharacterData SO 1장 생성
- [ ] 4 슬롯 SkillData wire-up 완료
- [ ] maxHealth / maxStamina 정함
- [ ] Player.prefab 의 PlayerCharacterBinder.character 교체
- [ ] 호스트·클라 양쪽 같은 캐릭 표시 (NGO 동기화 검증)
- [ ] 자율 역할구조 깨지지 않음 — 캐릭이 강제 역할 X

## 절대 사수

- **★ M11 = Kiyan 만** — M12+ 진입 전 새 캐릭 추가 X (사용자 직인 없으면)
- **★ 옛 코드 우선** — Kiyan.asset 직접 Read 후 변주
- **★ 환상 금지** — 새 인터페이스·매니저 자작 X. CharacterData 단일 패턴
- **자율 역할구조** — 캐릭이 빌드 가능성 강제하면 안 됨
- **메모리 즉시 갱신** — 새 캐릭 결정 = 그 턴에 박기
