---
name: add-skill
description: 차기작에 새 스킬 추가. SkillData 추상 + 구체 클래스 + SO 1장. 캐릭터 슬롯 wire-up. 환상 방지 SOP 강제.
argument-hint: "[skill-name] [type: attack|buff|dodge|passive]"
allowed-tools: Read, Grep, Glob, Bash, Write, Edit
---

# 새 스킬 추가 SOP

`SkillData : ScriptableObject, ISkill` (추상) 기반. 새 스킬 = 구체 클래스 1개 + SO 1장. 캐릭터 4슬롯에 wire-up.

## Input

`$ARGUMENTS` = `[skill-name] [type]` (예: `FireSlash attack`, `IronWill buff`)

## Step 1 — 옛 패턴 확인 (★ 환상 방지)

- `Assets/_Project/Scripts/Skills/ISkill.cs` — 인터페이스
- `Assets/_Project/Scripts/Skills/SkillData.cs` — 추상 SO 기반 클래스 (`ActivateOnServer(GameObject owner)` 추상)
- `Assets/_Project/Scripts/Skills/BasicMeleeSkill.cs` — 구체 어택 패턴 (Slash 속성, 콘 hitbox, ItemEffect 곱)
- `Assets/_Project/Scripts/Skills/BuffSkill.cs` + `TimedBuff.cs` — 버프 패턴
- `Assets/_Project/Scripts/Skills/RollDodgeSkill.cs` + `IInvincibilityTarget.cs` — 회피·무적
- `Assets/_Project/Scripts/Skills/KiyanShieldPassive.cs` — 패시브 패턴
- `Assets/_Project/Scripts/Skills/IKnockable.cs` — 넉백 인터페이스
- `Assets/_Project/Scripts/Skills/IDamageable.cs` — 데미지 + Faction (FF off)

> 못 찾는 파일 있으면 즉시 멈춤. 추측 X.

## Step 2 — 타입에 맞는 패턴 따르기

| type | 베이스 클래스 | 참조 |
|---|---|---|
| attack | `SkillData` 상속 + ActivateOnServer 안 데미지 | BasicMeleeSkill |
| buff | `SkillData` + TimedBuff 적용 | BuffSkill |
| dodge | `SkillData` + IInvincibilityTarget 적용 | RollDodgeSkill |
| passive | MonoBehaviour 또는 NetworkBehaviour. 캐릭터 prefab에 부착 | KiyanShieldPassive |

> active 스킬 새 타입 (texture·소환·시간 마법 등)이 옛 4타입 다 안 맞으면 — *그 자체가 인터페이스 부족 신호*. 사용자 직인 받고 새 인터페이스 만들기.

## Step 3 — 구체 클래스 작성

네임스페이스: `Elyqara.Skills`. 폴더: `Assets/_Project/Scripts/Skills/`.

```csharp
using UnityEngine;

namespace Elyqara.Skills
{
    [CreateAssetMenu(fileName = "[skill-name]", menuName = "Elyqara/Skills/[skill-name]")]
    public sealed class [SkillName]Skill : SkillData
    {
        // 옛 BasicMeleeSkill / BuffSkill / 등 패턴 따라 직렬화 필드 정의
        // [SerializeField] private float damageMultiplier = 1f; 등
        
        public override void ActivateOnServer(GameObject owner)
        {
            // 호스트 권위 처리만. 클라 X.
            // 옛 패턴: owner.GetComponent<PlayerResources>() / Inventory.GetTotalEffect() 등
        }
    }
}
```

> 필드 값은 *옛 스킬 변주*. 사용자 명시 값 받기. 추측 X.

## Step 4 — SO Asset 생성

Unity Editor: `Assets/_Project/Data/Skills/` 우클릭 → Create / Elyqara / Skills / [skill-name].

또는 Editor 스크립트로 자동화 — `Editor/Phase13Setup.cs` 패턴 따라.

## Step 5 — 캐릭터 슬롯 wire-up

`CharacterData` 의 4슬롯 중 하나에 새 SO 연결:
- `primarySkill` — 마우스 좌클릭 (기본공격)
- `secondarySkill` — 마우스 우클릭
- `qSkill` — Q 키
- `dodgeSkill` — 스페이스바

Kiyan 예시: `Assets/_Project/Data/Characters/Kiyan.asset` 에서 슬롯 교체.

> M11 단순화: Kiyan 1캐릭 / 슬롯 4개 고정. 신 스킬 풀 해금은 M12+ (`memory/kiyan_phase13_2_complete_20260505.md` 참조).

## Step 6 — 검증

- [ ] `SkillData` 상속한 sealed class
- [ ] `ActivateOnServer` 구현 (호스트 권한)
- [ ] CooldownSeconds, StaminaCost 설정
- [ ] FF off — `IDamageable.Faction` 같으면 skip (BasicMeleeSkill 패턴 참조)
- [ ] ItemEffect 곱 적용 (Slash/Blunt 데미지 % 증가)
- [ ] SO Asset 생성 + 캐릭터 슬롯 wire-up

## 절대 사수

- **★ 옛 코드 우선** — 4타입(attack/buff/dodge/passive) 옛 패턴 직접 Read 후 진행
- **★ 환상 금지** — 새 인터페이스 / 새 매니저 *자작 X*. 부족 신호 = 사용자 보고
- **단일 책임** — 스킬 한 클래스 = 한 행동. 복합 행동은 여러 클래스 합성
- **메모리 즉시 갱신** — 사용자 직인 값·결정 = 그 턴에 메모리 박기
