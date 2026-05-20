---
name: check-pattern
description: 새 기능 추가 전 옛 코드 grep/Read 강제. 카테고리별 인터페이스·매니저·데이터 패턴 보고. 환상 폭발 재발 방지 1번 도구.
argument-hint: "[category: enemy|skill|item|character|dungeon|ui|network|combat]"
allowed-tools: Read, Grep, Glob, Bash
---

# 옛 패턴 확인 SOP

차기작 절대 원칙 1번 (*옛 코드 우선 확인*) 의 손잡이. 새 기능 추가 *전에* 항상 실행. **읽기만**.

> 비슷한 일을 *백그라운드*로 하고 싶으면 `code-pattern-checker` sub-agent 사용. 이 skill 은 *메인 클로드가 직접* 확인하는 워크플로우.

## Input

`$ARGUMENTS` = `[category]`

| category | 카테고리 |
|---|---|
| enemy | 적 / EnemyController / EnemyData |
| skill | 스킬 / SkillData / ISkill |
| item | 아이템 / ItemData / ItemEffect / Inventory |
| character | 캐릭터 / CharacterData |
| dungeon | 던전 / Room / DungeonManager / RuntimeDungeonGenerator |
| ui | UI / InventoryUI / GameOverUI / VictoryUI / PlayerHUD |
| network | NGO / NetworkBootstrap / NetworkBehaviour |
| combat | 데미지 / IDamageable / Faction / hitbox |

## Step 1 — 카테고리 파일 grep

`Assets/_Project/Scripts/[Category]/` 안 모든 .cs grep:
- 인터페이스 정의 (IEnemy / ISkill / IItem 등)
- 추상 클래스 / SO 베이스 (EnemyData / SkillData / ItemData / CharacterData)
- 단일 매니저 (EnemySpawner / Inventory / DungeonManager / GameStateManager)
- 컨트롤러 (EnemyController : NetworkBehaviour 등)

## Step 2 — 데이터 폴더 확인

`Assets/_Project/Data/[Category]/` 안 .asset 목록. 옛 데이터가 패턴 예시.

## Step 3 — 인터페이스 / 매니저 / 데이터 패턴 보고

다음 형식:

```markdown
## 카테고리: [category]

### 발견된 옛 패턴
- **인터페이스**: [목록 + 경로]
- **추상 클래스 / SO 베이스**: [목록]
- **단일 매니저**: [목록]
- **컨트롤러 / NetworkBehaviour**: [목록]
- **데이터 .asset**: [옛 SO 파일들]
- **네임스페이스**: Elyqara.[Category]
- **asmdef**: Elyqara.[Category]

### 새 기능 추가 SOP (이 카테고리)
1. [SO 1장 만들기] or [구체 클래스 + SO 1장]
2. [어디 등록]
3. [어떤 인터페이스 구현]

### 빠진 것 / 위험 신호
- [있으면 명시. 예: 인터페이스 없음 → 새로 만들어야 → 사용자 직인 필요]
```

## Step 4 — 위반 위험 환기

다음 패턴 발견 시 사용자에게 경고:
- 같은 카테고리에 매니저 2개 이상 (단일 매니저 위반)
- ScriptableObject 패턴이 옛 코드와 다름 (컨벤션 깨짐)
- NetworkBehaviour 아닌 곳에서 게임 상태 변경 (호스트 권한 위반)
- 인터페이스 우회 (직접 컴포넌트 GetComponent 남발)

## 절대 사수

- **읽기만**. 코드 수정 X. 보고만.
- **간결**. 한 카테고리 한 메시지 안.
- **사실만**. grep / Read 결과만. 추측 X.
- **인터페이스 부족 신호 강조** — 새 인터페이스 필요하면 사용자 직인 필요.
- 글로벌 코딩 4원칙 (Karpathy) 위반 가능성도 같이 환기 — 특히 *Simplicity First* / *Surgical Changes*.
