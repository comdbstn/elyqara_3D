---
name: add-enemy
description: 차기작에 새 적 추가. EnemyData SO 1장 + (필요 시) DropTableData. 코드 수정은 거의 0줄 지향. 옛 코드 환상 방지 SOP 강제.
argument-hint: "[enemy-name]"
allowed-tools: Read, Grep, Glob, Bash, Write, Edit
---

# 새 적 추가 SOP

차기작의 단일 매니저·인터페이스 우선·데이터 주도 원칙 그대로 따른다. 새 적 = `EnemyData.asset` 1장 + 필요 시 `DropTableData.asset` 1장. 코드 추가는 *기존 패턴으로 불가능할 때만*.

## Input

`$ARGUMENTS` = `[enemy-name]` (예: `Skeleton`, `BossWisp`)

## Step 1 — 옛 패턴 확인 (★ 환상 방지)

다음 파일 Read하고 패턴 확인. 추측 X.

- `Assets/_Project/Scripts/Enemies/EnemyData.cs` — SO 필드 구조 (Identity / Resources / Drops / Movement / Aggro / Attack 4-phase / Hitbox)
- `Assets/_Project/Scripts/Enemies/EnemyController.cs` — Souls-like 6-state FSM (Idle/Chase/Anticipation/Active/Recovery/Dead)
- `Assets/_Project/Scripts/Enemies/IEnemy.cs` — 인터페이스
- `Assets/_Project/Data/Enemies/Wisp.asset` — 첫 적 데이터 참조 (다른 적은 이 값 변주로)
- `Assets/_Project/Scripts/Items/DropTableData.cs` — 드랍 가중치 Roll 패턴

> 만약 위 어느 파일이라도 못 찾으면 **즉시 멈추고 사용자에게 보고**. 추측해서 진행 X.

## Step 2 — 새 인터페이스가 필요한가?

새 적이 기존 6-state FSM + 4-phase attack 으로 표현 가능한가?
- ✅ 가능 → 데이터만 추가. Step 3.
- ❌ 불가 (특수 행동 — 텔레포트·소환·페이즈 변화 등) → **새 행동은 인터페이스 부족 신호**. 사용자에게 보고하고 *어떤 새 인터페이스가 필요한지* 제안. 옛 코드 옆에 새 매니저 만들지 말 것.

## Step 3 — 데이터 추가

`Assets/_Project/Data/Enemies/[enemy-name].asset` 생성.

EnemyData SO 필드를 옛 Wisp.asset 기준으로 변주:
- `enemyName` — 표시 이름
- `maxHealth` — 적당 데미지에 비례
- `dropTable` — 필요 시 `DropTableData.asset` 1장 추가
- `moveSpeed`, `chaseStopDistance` — Player 박치기 방지
- `aggroRadius` / `deaggroRadius` — 8 / 14 기본
- `attackRange`, `attackDamage`
- `attackWindup` / `attackActive` / `attackRecovery` / `attackCooldown` — 4-phase telegraph
- `hitboxHalfAngleDeg` — 콘 반각

> 값은 *기존 Wisp 변주*로 정한다. 임의 값 추측 X — 사용자에게 *"Wisp 대비 어떻게 다른지"* 묻거나 명시 값 받기.

## Step 4 — 프리팹 (필요 시)

기존 적 프리팹이 모델·VFX 다르면 새 프리팹 필요. Wisp 프리팹 복제 + visual 교체 + `EnemyController.enemyData` 필드에 새 SO 연결.

## Step 5 — Spawn 등록

옛 패턴: `EnemySpawner` 가 호스트 시작 시 1마리. 새 적 spawn 위치·시점은 던전 단계 또는 보스방 트리거로.

> 13-1 자동 생성 단계 이후 — `RuntimeDungeonGenerator` 가 적 spawn 처리. 그 패턴 그대로 따라야 함. **새 spawn 시스템 만들지 말 것**.

## Step 6 — 검증

- [ ] EnemyData SO 1장 생성 + 필드 다 채움
- [ ] 옛 6-state FSM·4-phase attack 으로 표현 가능한 적인지 확인
- [ ] 새 인터페이스 안 만들었는가? (만들면 사용자 직인 필요)
- [ ] 호스트 권한 위반 X (적 행동은 EnemyController : NetworkBehaviour 안에서만)
- [ ] 데미지 파이프라인 `IDamageable.Faction = Enemy` 정합
- [ ] DropTableData 등록 — 아이템 드랍 원하면

## 절대 사수

- **★ 옛 코드 우선** — Step 1 안 거치고 진행 X
- **★ 환상 금지** — 파일 경로 / 필드명 / 값은 *직접 Read한 것만*. 추측 표현 (*"보통 그렇다"*) 떠오르면 멈춤
- **데이터 주도** — 코드 추가 0줄 지향. 추가 필요하면 인터페이스 부족 신호
- 메모리 즉시 갱신 (새 적 추가 결정·값 직인은 `~/.claude/projects/-Users-jeong-yunsu-Elyqara-3D/memory/`)
