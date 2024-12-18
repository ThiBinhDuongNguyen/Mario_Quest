using Microsoft.Xna.Framework.Graphics;
using System;
using Microsoft.Xna.Framework;

namespace FinalProjectGame
{
	internal class Enemy
	{
		public Vector2 Position; // Enemy position
		public float Speed; // Movement speed
		public Rectangle MovementBounds; // Movement boundaries for the enemy

		public Enemy(Vector2 position, float speed, Rectangle movementBounds)
		{
			Position = position;
			Speed = speed;
			MovementBounds = movementBounds;
		}

		public void Update()
		{
			// Update enemy position
			Position.X += Speed;

			// Reverse direction if hitting the bounds
			if (Position.X < MovementBounds.X || Position.X + 50 > MovementBounds.X + MovementBounds.Width)
			{
				Speed *= -1; // Reverse speed
			}
		}

		public void Draw(SpriteBatch spriteBatch, Texture2D enemyTexture)
		{
			// Draw the enemy at its current position
			spriteBatch.Draw(enemyTexture, Position, Color.White);
		}
	}
}
