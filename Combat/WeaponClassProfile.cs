namespace projecthorizonscs.Combat;

public sealed class WeaponClassProfile
{
	public static readonly WeaponClassProfile Default = new();

	public float AttackAnimationSpeedMultiplier { get; init; } = 1f;
	public float MoveSpeedMultiplier { get; init; } = 1f;
	public float DashCooldownMultiplier { get; init; } = 1f;
	public int MeleeDamageBonus { get; init; }
	public int ProjectileDamageBonus { get; init; }
	public int ProjectileCount { get; init; } = 1;
	public float ProjectileSpreadDegrees { get; init; }
	public float ProjectileSpeedMultiplier { get; init; } = 1f;
	public float ProjectileScaleMultiplier { get; init; } = 1f;

	public static WeaponClassProfile FromItemClass(Item.ITEM_CLASS itemClass)
	{
		return itemClass switch
		{
			Item.ITEM_CLASS.Melee => new WeaponClassProfile
			{
				AttackAnimationSpeedMultiplier = 1.1f
			},
			Item.ITEM_CLASS.HeavyMelee => new WeaponClassProfile
			{
				AttackAnimationSpeedMultiplier = 0.82f,
				MoveSpeedMultiplier = 0.94f,
				DashCooldownMultiplier = 1.12f,
				MeleeDamageBonus = 1
			},
			Item.ITEM_CLASS.Mage => new WeaponClassProfile
			{
				AttackAnimationSpeedMultiplier = 1.02f,
				ProjectileSpeedMultiplier = 1.05f
			},
			Item.ITEM_CLASS.Wizard => new WeaponClassProfile
			{
				AttackAnimationSpeedMultiplier = 0.92f,
				MoveSpeedMultiplier = 0.95f,
				ProjectileDamageBonus = 1,
				ProjectileCount = 2,
				ProjectileSpreadDegrees = 10f,
				ProjectileSpeedMultiplier = 0.95f,
				ProjectileScaleMultiplier = 1.1f
			},
			Item.ITEM_CLASS.Ranged => new WeaponClassProfile
			{
				AttackAnimationSpeedMultiplier = 1.08f,
				ProjectileDamageBonus = 1,
				ProjectileSpreadDegrees = 4f,
				ProjectileSpeedMultiplier = 1.08f
			},
			Item.ITEM_CLASS.Archer => new WeaponClassProfile
			{
				AttackAnimationSpeedMultiplier = 0.96f,
				ProjectileSpeedMultiplier = 1.2f,
				ProjectileScaleMultiplier = 0.95f
			},
			Item.ITEM_CLASS.Bommet => new WeaponClassProfile
			{
				AttackAnimationSpeedMultiplier = 0.94f,
				MoveSpeedMultiplier = 0.97f,
				ProjectileDamageBonus = 1,
				ProjectileScaleMultiplier = 1.15f
			},
			_ => Default
		};
	}
}
