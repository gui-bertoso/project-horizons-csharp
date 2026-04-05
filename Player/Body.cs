using Godot;
using projecthorizonscs.Autoload;

namespace projecthorizonscs.Player;

public partial class Body : Node2D
{
	private Player _player;

	private Sprite2D _backLayer0Sprite;
	private Sprite2D _backLayer1Sprite;
	private Sprite2D _backLayer2Sprite;
	private Sprite2D _backLayer3Sprite;
	private Sprite2D _backLayer4Sprite;
	private Sprite2D _backLayer5Sprite;
	private Sprite2D _backLayer6Sprite;

	private Sprite2D _sideLayer0Sprite;
	private Sprite2D _sideLayer1Sprite;
	private Sprite2D _sideLayer2Sprite;
	private Sprite2D _sideLayer3Sprite;
	private Sprite2D _sideLayer4Sprite;
	private Sprite2D _sideLayer5Sprite;
	private Sprite2D _sideLayer6Sprite;

	private AnimationPlayer _animationPlayer;

	public override void _Ready()
	{
		_backLayer0Sprite = GetNode<Sprite2D>("Back/Layer0");
		_backLayer1Sprite = GetNode<Sprite2D>("Back/Layer1");
		_backLayer2Sprite = GetNode<Sprite2D>("Back/Layer2");
		_backLayer3Sprite = GetNode<Sprite2D>("Back/Layer3");
		_backLayer4Sprite = GetNode<Sprite2D>("Back/Layer4");
		_backLayer5Sprite = GetNode<Sprite2D>("Back/Layer5");
		_backLayer6Sprite = GetNode<Sprite2D>("Back/Layer6");

		_sideLayer0Sprite = GetNode<Sprite2D>("Side/Layer0");
		_sideLayer1Sprite = GetNode<Sprite2D>("Side/Layer1");
		_sideLayer2Sprite = GetNode<Sprite2D>("Side/Layer2");
		_sideLayer3Sprite = GetNode<Sprite2D>("Side/Layer3");
		_sideLayer4Sprite = GetNode<Sprite2D>("Side/Layer4");
		_sideLayer5Sprite = GetNode<Sprite2D>("Side/Layer5");
		_sideLayer6Sprite = GetNode<Sprite2D>("Side/Layer6");

		_animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		_player = (Player)GetParent();

		Globals.I.LocalPlayerBody = this;
	}

	public override void _Process(double delta)
	{
		AnimationBehavior();
	}

	private void AnimationBehavior()
	{
		if (_player.IsAttacking())
			return;

		if (_animationPlayer.CurrentAnimation == "collect_side" ||
			_animationPlayer.CurrentAnimation == "collect_back")
		{
			return;
		}

		var velocity = _player.Velocity;
		if (velocity != Vector2.Zero)
		{
			if (_player.CurrentSide == 1)
				_animationPlayer.Play("walk_forward_side");
			else
				_animationPlayer.Play("walk_forward_back");
		}
		else
		{
			if (_player.CurrentSide == 1)
				_animationPlayer.Play("idle_side");
			else
				_animationPlayer.Play("idle_back");
		}
	}

	public void SetArmorTexture(Item item)
	{
		switch (item.ItemType)
		{
			case Item.ITEM_TYPE.HeadArmor:
				SetHeadTexture(item.ArmorSpriteSheet);
				break;
			case Item.ITEM_TYPE.BodyArmor:
				SetBodyTexture(item.ArmorSpriteSheet);
				break;
			case Item.ITEM_TYPE.FootArmor:
				SetFootTexture(item.ArmorSpriteSheet);
				break;
		}
	}

	private void SetHeadTexture(Texture2D texture)
	{
		_backLayer5Sprite.Texture = texture;
		_sideLayer5Sprite.Texture = texture;

		_backLayer6Sprite.Texture = texture;
		_sideLayer6Sprite.Texture = texture;
	}

	private void SetBodyTexture(Texture2D texture)
	{
		_backLayer0Sprite.Texture = texture;
		_sideLayer0Sprite.Texture = texture;

		_backLayer3Sprite.Texture = texture;
		_sideLayer3Sprite.Texture = texture;

		_backLayer2Sprite.Texture = texture;
		_sideLayer2Sprite.Texture = texture;
	}

	private void SetFootTexture(Texture2D texture)
	{
		_backLayer4Sprite.Texture = texture;
		_sideLayer4Sprite.Texture = texture;

		_backLayer1Sprite.Texture = texture;
		_sideLayer1Sprite.Texture = texture;
	}
}
