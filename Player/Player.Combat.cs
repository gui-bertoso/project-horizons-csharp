using Godot;
using projecthorizonscs.Combat;

namespace projecthorizonscs.Player;

public partial class Player
{
	private void HandleActionInput()
	{
		if (_isCollecting)
			return;

		string weaponClass = _hand.GetWeaponClass();

		if (weaponClass == "ranged")
		{
			HandleRangedAttackInput();
			return;
		}

		if (weaponClass == "sword")
		{
			HandleSwordAttack();
			return;
		}

		_isAttacking = false;
	}

	private void HandleSwordAttack()
	{
		if (_isSwordAttacking || !Input.IsActionJustPressed("action"))
			return;

		_isSwordAttacking = true;
		_isAttacking = true;
		_animationPlayer.SpeedScale = GetCombatProfile().AttackAnimationSpeedMultiplier;

		if (_currentSide == 1)
			_animationPlayer.Play("sword_attack_side");
		else
			_animationPlayer.Play("sword_attack_back");
	}

	private void HandleRangedAttackInput()
	{
		_isAttacking = Input.IsActionPressed("action");
		_animationPlayer.SpeedScale = GetCombatProfile().AttackAnimationSpeedMultiplier;

		if (_rangedState == RangedAttackState.Ending)
		{
			if (_animationPlayer.CurrentAnimation != "ranged_attack_down_side" &&
				_animationPlayer.CurrentAnimation != "ranged_attack_down_back")
			{
				ResetRangedAttack();
			}
		}

		if (_rangedState == RangedAttackState.Starting)
		{
			if (_animationPlayer.CurrentAnimation != "ranged_attack_up_side" &&
				_animationPlayer.CurrentAnimation != "ranged_attack_up_back")
			{
				ResetRangedAttack();
			}
		}

		if (_isAttacking && _rangedState == RangedAttackState.None)
		{
			StartRangedAttack();
			return;
		}

		if (!_isAttacking && _rangedState == RangedAttackState.Looping)
			EndRangedAttack();
	}

	private void StartRangedAttack()
	{
		_attackSide = _currentSide;
		_rangedState = RangedAttackState.Starting;

		if (_attackSide == 1)
			_animationPlayer.Play("ranged_attack_up_side");
		else
			_animationPlayer.Play("ranged_attack_up_back");
	}

	private void EnterRangedLoop()
	{
		_rangedState = RangedAttackState.Looping;

		if (_attackSide == 1)
			_animationPlayer.Play("ranged_attack_loop_side");
		else
			_animationPlayer.Play("ranged_attack_loop_back");
	}

	private void EndRangedAttack()
	{
		if (_rangedState == RangedAttackState.Ending || _rangedState == RangedAttackState.None)
			return;

		_rangedState = RangedAttackState.Ending;

		if (_attackSide == 1)
			_animationPlayer.Play("ranged_attack_down_side");
		else
			_animationPlayer.Play("ranged_attack_down_back");
	}

	private void ResetRangedAttack()
	{
		_rangedState = RangedAttackState.None;
		_animationPlayer.SpeedScale = 1f;
	}

	private void OnAnimationFinished(StringName animName)
	{
		string anim = animName.ToString();

		switch (anim)
		{
			case "ranged_attack_up_side":
			case "ranged_attack_up_back":
				if (_isAttacking)
					EnterRangedLoop();
				else
					EndRangedAttack();
				break;

			case "ranged_attack_down_side":
			case "ranged_attack_down_back":
				ResetRangedAttack();

				if (_isAttacking)
					StartRangedAttack();

				break;

			case "collect_side":
			case "collect_back":
				_isCollecting = false;
				_animationPlayer.SpeedScale = 1f;
				break;

			case "sword_attack_side":
			case "sword_attack_back":
				_isSwordAttacking = false;
				_isAttacking = false;
				_animationPlayer.SpeedScale = 1f;
				break;
		}
	}

	public async void CollectItem(PhysicsItem node)
	{
		if (_isCollecting)
			return;

		_isCollecting = true;
		_isAttacking = false;
		_animationPlayer.SpeedScale = 1f;

		if (_rangedState != RangedAttackState.None)
			ResetRangedAttack();

		if (_currentSide == 1)
			_animationPlayer.Play("collect_side");
		else
			_animationPlayer.Play("collect_back");

		await ToSignal(GetTree().CreateTimer(_animationPlayer.CurrentAnimationLength), SceneTreeTimer.SignalName.Timeout);
		node.Collect();
	}

	private WeaponClassProfile GetCombatProfile()
	{
		return _hand?.GetCombatProfile() ?? WeaponClassProfile.Default;
	}
}
