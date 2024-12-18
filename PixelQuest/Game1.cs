using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;

namespace FinalProjectGame
{
	public class Game1 : Game
	{
		private GraphicsDeviceManager _graphics;
		private SpriteBatch _spriteBatch;

		private Player _player;
		private Platform[] _platforms;
		private Coin[] _coins;
		private Enemy[] _enemies;

		private Texture2D _playerTexture, _platformTexture, _coinTexture, _enemyTexture, _backgroundTexture;
		private SpriteFont _font;

		private Vector2 _backgroundOffset;
		private int _score;
		private bool _gameOver;
		private bool _gameOverAtEnd;
		private bool _isGameFrozen;
		private const int Gravity = 1;
		private const int JumpStrength = -15;

		//for sound
		private Song _menuMusic;
		private Song _gameplayMusic;
		private Song _collisionSound;
		private Song _gameOverSound;

		private bool _musicPlaying;



		private double _elapsedTime; // It's like for elapsed time in the game
		private string _endMessage; //It's like shows message for Game Over	
		private const double TimeLimit = 30.0; // It's like remaining time in the game
		private bool _shouldRefresh = false;  // It's for screen refresh and it's boolean value 

		// Added variables for menu handling
		private string _currentGameState = "StartMenu"; // "StartMenu", "Playing", "GameOverMenu"  

		public Game1()
		{
			_graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			IsMouseVisible = true;
		}

		protected override void Initialize()
		{
			_graphics.PreferredBackBufferWidth = 800;
			_graphics.PreferredBackBufferHeight = 600;
			_graphics.ApplyChanges();
			base.Initialize();
		}

		protected override void LoadContent()
		{

			// for sound 
			_menuMusic = Content.Load<Song>("gameMenu");
			_gameplayMusic = Content.Load<Song>("inGame");
			_collisionSound = Content.Load<Song>("enemyHit");
			_gameOverSound = Content.Load<Song>("gameEnd");

			MediaPlayer.IsRepeating = true;
			//initalising it 
			PlayMusic(_menuMusic);


			_spriteBatch = new SpriteBatch(GraphicsDevice);

			_playerTexture = Content.Load<Texture2D>("guidoPlayer");
			_platformTexture = Content.Load<Texture2D>("platform");
			_coinTexture = Content.Load<Texture2D>("coin");
			_enemyTexture = Content.Load<Texture2D>("deathcapEnemy");
			_backgroundTexture = Content.Load<Texture2D>("background");
			_font = Content.Load<SpriteFont>("DefaultFont");

			_player = new Player(new Vector2(100, 500));

			_platforms = new[]
			{
				new Platform(new Rectangle(0, 580, 800, 20)),
				new Platform(new Rectangle(150, 450, 200, 20)),
				new Platform(new Rectangle(400, 350, 200, 20)),
				new Platform(new Rectangle(650, 250, 150, 20))
			};

			_coins = new[]
			{
				new Coin(new Vector2(150 + 200 / 2 - 10, 450 - 20)),
				new Coin(new Vector2(400 + 200 / 2 - 10, 350 - 20)),
				new Coin(new Vector2(650 + 150 / 2 - 10, 250 - 20))
			};

			_enemies = new[]
			{
				new Enemy(new Vector2(180, 430 - _enemyTexture.Height), 1, _platforms[1].Rectangle),
				new Enemy(new Vector2(470, 330 - _enemyTexture.Height), -1, _platforms[2].Rectangle),
				new Enemy(new Vector2(720, 230 - _enemyTexture.Height), 1, _platforms[3].Rectangle)
			};
		}

		private void PlayMusic(Song song)
		{
			MediaPlayer.Stop();
			MediaPlayer.Play(song);
			_musicPlaying = true;
		}


		protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            var keyboardState = Keyboard.GetState();

            // Handle game states
            if (_currentGameState == "StartMenu")
            {
				if (!_musicPlaying)
				{
					PlayMusic(_menuMusic);
				}

				if (keyboardState.IsKeyDown(Keys.Enter))
                {
                    _currentGameState = "Playing";
                    _elapsedTime = 0;
					PlayMusic(_gameplayMusic);

				}
				else if (keyboardState.IsKeyDown(Keys.Escape))
                {
                    Exit();
                }
                return;  // It's to skip the rest of the update logic which is assosicated with StartMenu
			}

            if (_currentGameState == "GameOverMenu")
            {
				if (!_musicPlaying)
				{
					PlayMusic(_gameOverSound); // Play end music
				}

				if (keyboardState.IsKeyDown(Keys.R))
                {
                    RestartGame();
                }
                else if (keyboardState.IsKeyDown(Keys.Escape))
                {
                    Exit();
                }
                return;  // It's to skip the rest of the update logic which is assosicated with GameOverMenu
			}

			// It update elapsed time while you are playing the game
            if (_currentGameState == "Playing")
            {
                _elapsedTime += gameTime.ElapsedGameTime.TotalSeconds; // Basically increment elapsed time
																	   // It's check for the remaining time in the game
				if (_elapsedTime >= TimeLimit)
                {
                    _endMessage = "Time's up! Game over.";
                    _currentGameState = "GameOverMenu";
                    return;
                }
            }

            // Prevent updates if the game is frozen
            if (_isGameFrozen)
                return;

            //It's logic for movement and jumping 
            if (keyboardState.IsKeyDown(Keys.Left))
            {
                _player.Position.X -= 3;
            }

            if (keyboardState.IsKeyDown(Keys.Right))
            {
                _player.Position.X += 3;
            }

            // Clamp player's horizontal position
            _player.Position.X = MathHelper.Clamp(_player.Position.X, 0, _graphics.PreferredBackBufferWidth - _playerTexture.Width);

            // Jumping logic - consolidated
            bool canJump = !_player.IsJumping;
            if (keyboardState.IsKeyDown(Keys.Space) && canJump)
            {
                // Regular jump
                if (!keyboardState.IsKeyDown(Keys.Enter))
                {
                    _player.Velocity.Y = JumpStrength; // Use constant for consistent jump
                    _player.IsJumping = true;
                }
                // Boosted jump with directional movement
                else
                {
                    _player.Velocity.Y = JumpStrength - 5; // Slightly stronger jump
                    _player.IsJumping = true;

                    // Directional movement during boosted jump
                    if (keyboardState.IsKeyDown(Keys.Right))
                    {
                        _player.Position.X += 5;
                    }
                    else if (keyboardState.IsKeyDown(Keys.Left))
                    {
                        _player.Position.X -= 5;
                    }
                }
            }

            //It apply gravity consistently
            _player.Velocity.Y += Gravity;
            _player.Position += _player.Velocity;

            // Check if player reaches the right edge of the screen or not
            if (_player.Position.X >= _graphics.PreferredBackBufferWidth - _playerTexture.Width)
            {
                _shouldRefresh = true; // Trigger screen refresh
            }

            // Handle screen refresh
            if (_shouldRefresh)
            {
                RefreshGameElements();
                _shouldRefresh = false; // Reset the flag
            }

			bool isOnGround = false;
			bool isOnPlatform = false;

			//It's for Platform Collision Detection (Top Surface)
			foreach (var platform in _platforms)
			{
				// Top Surface Collision part
				if (_player.Position.Y + _playerTexture.Height >= platform.Rectangle.Y && // player's feet are at or below the platform's top edge
                    _player.Position.Y + _playerTexture.Height <= platform.Rectangle.Y + platform.Rectangle.Height && // player horizontally aligned with the platform
                    _player.Position.X + _playerTexture.Width > platform.Rectangle.X &&
					_player.Position.X < platform.Rectangle.X + platform.Rectangle.Width &&
					_player.Velocity.Y >= 0) // Player must be falling
				{
					_player.Position.Y = platform.Rectangle.Y - _playerTexture.Height;
					_player.Velocity.Y = 0;
					_player.IsJumping = false; // Allow jumping again
					isOnPlatform = true;
					break;
				}
				else if (
					// Bottom Surface Collision (prevent jumping through) part
					_player.Position.Y < platform.Rectangle.Y + platform.Rectangle.Height &&
					_player.Position.Y > platform.Rectangle.Y &&
					_player.Position.X + _playerTexture.Width > platform.Rectangle.X &&
					_player.Position.X < platform.Rectangle.X + platform.Rectangle.Width &&
					_player.Velocity.Y < 0) // Player is moving upwards
				{
					_player.Velocity.Y = 0; // Stop upward movement
				}
			}

			// Ground Collision part
			if (_player.Position.Y + _playerTexture.Height >= 580 && _player.Position.X >= 0) // player dont fall of the screen
			{
				_player.Position.Y = 580 - _playerTexture.Height;
				_player.Velocity.Y = 0;
				_player.IsJumping = false;
				isOnGround = true;
			}

			// Reset vertical velocity if on ground or platform
			if (isOnPlatform || isOnGround)
			{
				_player.Velocity.Y = 0; // If the player is on a platform or the ground
                _player.IsJumping = false; // they won’t move vertically (fall or jump unintentionally
            }


			// Vertical position clamping
			_player.Position.Y = MathHelper.Clamp(_player.Position.Y, 0, 489);

            // Coin collection
            foreach (var coin in _coins)
            {
                if (!coin.Collected)
                {
                    // Create bounding boxes for the player and the coin
                    Rectangle playerBounds = new Rectangle(
                        (int)_player.Position.X,
                        (int)_player.Position.Y,
                        _playerTexture.Width,
                        _playerTexture.Height
                    );

                    Rectangle coinBounds = new Rectangle(
                        (int)coin.Position.X,
                        (int)coin.Position.Y,
                        _coinTexture.Width,
                        _coinTexture.Height
                    );

                    // Check if the player's bounding box intersects the coin's bounding box
                    if (playerBounds.Intersects(coinBounds))
                    {
                        // Ensure the player is on the same platform as the coin
                        foreach (var platform in _platforms)
                        {
                            if (_player.Position.Y + _playerTexture.Height == platform.Rectangle.Y &&
                                coin.Position.X + _coinTexture.Width > platform.Rectangle.X &&
                                coin.Position.X < platform.Rectangle.X + platform.Rectangle.Width)
                            {
                                coin.Collected = true;
                                _score++;
                            }
                        }
                    }
                }
            }

            // Move the background with the average position of the enemies
            //_backgroundOffset.X = -(_enemies[0].Position.X + _enemies[1].Position.X + _enemies[2].Position.X) / 3 % _graphics.PreferredBackBufferWidth;

            // Enemy movement and collision
            foreach (var enemy in _enemies)
            {
                // If the game is frozen, it stop enemy movement
                if (_isGameFrozen)
                    continue;

                // Enemy movement logic
                enemy.Position.X += enemy.Speed;

                // Reverse enemy direction when hitting bounds
                if (enemy.Position.X < enemy.MovementBounds.X ||
                    enemy.Position.X + _enemyTexture.Width > enemy.MovementBounds.X + enemy.MovementBounds.Width)
                {
                    enemy.Speed *= -1;
                }

                //It's to check collision between player and enemy
                if (_player.Position.X < enemy.Position.X + _enemyTexture.Width &&
                    _player.Position.X + _playerTexture.Width > enemy.Position.X &&
                    _player.Position.Y < enemy.Position.Y + _enemyTexture.Height &&
                    _player.Position.Y + _playerTexture.Height > enemy.Position.Y)
                {
                    _isGameFrozen = true; // Freeze the game
                    _player.Velocity = Vector2.Zero; // Stop player movement
                    _currentGameState = "GameOverMenu";
					PlayMusic(_gameOverSound);
					
					break; // Exit loop after collision
                }
            }

            //It stop player movement if the game is frozen
            if (_isGameFrozen)
            {
                _player.Velocity = Vector2.Zero; // Prevent player from moving
            }

            if (_isGameFrozen)
            {
                _endMessage = $"You collided with an enemy!";
                _currentGameState = "GameOverMenu";
            }
            else if (_gameOverAtEnd)
            {
                _endMessage = $"Good job! Game over.";
                _currentGameState = "GameOverMenu";
            }

            base.Update(gameTime);
        }
        
        private void RefreshGameElements()
		{
			var random = new Random();

            // Reset player position near the left side of the screen, ready to begin the game.
            _player.Position = new Vector2(100, 500);

			// Generate random platforms
			_platforms = new Platform[4]; // arr 4 pf give rand dimension + position
			for (int i = 0; i < _platforms.Length; i++)
			{
				int platformWidth = random.Next(150, 300); // Random width between 150 and 300
				int platformX = random.Next(50, _graphics.PreferredBackBufferWidth - platformWidth - 50); // Random X within screen bounds
				int platformY = 200 + i * 100; // Space platforms vertically to avoid overlap

				_platforms[i] = new Platform(new Rectangle(platformX, platformY, platformWidth, 20));
			}

			// Generate coins on platforms 1 coin per platform
			_coins = new Coin[_platforms.Length]; 
			for (int i = 0; i < _platforms.Length; i++)
			{
				var platform = _platforms[i]; // Gets the current platform to determine where to place the coin.
                int coinX = platform.Rectangle.X + platform.Rectangle.Width / 2 - 10; // Center coin on the platform
				int coinY = platform.Rectangle.Y - 20; // Place coin slightly above the platform

				_coins[i] = new Coin(new Vector2(coinX, coinY));
			}

			// Generate enemies on platforms
			_enemies = new Enemy[_platforms.Length];
			for (int i = 0; i < _platforms.Length; i++)
			{
				var platform = _platforms[i];
				int enemyX = random.Next(platform.Rectangle.X, platform.Rectangle.X + platform.Rectangle.Width - 50); // Random X within platform bounds
				int enemyY = platform.Rectangle.Y - _enemyTexture.Height; // Place enemy on top of the platform
				

				_enemies[i] = new Enemy(new Vector2(enemyX, enemyY), 1, platform.Rectangle);
			}
		}

			private void RestartGame()
		{
			_player.Position = new Vector2(100, 500);
			_score = 0; //It reset score
			_elapsedTime = 0; //It reset elapsed time
			_isGameFrozen = false;
			_gameOver = false;
			_gameOverAtEnd = false;

			foreach (var coin in _coins)
				coin.Collected = false;

			_currentGameState = "StartMenu"; //It reset to Start Menu
			PlayMusic(_menuMusic);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);

			_spriteBatch.Begin();

			if (_currentGameState == "StartMenu")
			{
				DrawStartMenu();
			}
			else if (_currentGameState == "Playing")
			{
				DrawPlayingState();
			}
			else if (_currentGameState == "GameOverMenu")
			{
				DrawGameOverMenu();
			}

			_spriteBatch.End();

			base.Draw(gameTime);
		}

		private void DrawStartMenu()
		{
			// Draw a semi-transparent background for the menu
			_spriteBatch.Draw(_backgroundTexture, new Vector2(0, 0), new Color(0, 0, 0, 150));

			// Title text

			_spriteBatch.DrawString(_font, "Welcome to Our Game!", new Vector2(180, 600 / 4), Color.Yellow);

			// Start button text
			var startTextPosition = new Vector2(180, 600 / 2 - 50); // Center of screen horizontally, below title
			_spriteBatch.DrawString(_font, "Start Game (press enter)", startTextPosition, Color.White);

			// Exit button text
			var exitTextPosition = new Vector2(250, 600 / 2 + 50); // Center of screen horizontally, below start text
			_spriteBatch.DrawString(_font, "Exit (press esc)", exitTextPosition, Color.White);
		}


		private void DrawPlayingState()
		{
			_spriteBatch.Draw(_backgroundTexture, new Rectangle(0, 0, 800, 600), Color.White);

			foreach (var platform in _platforms)
			{
				_spriteBatch.Draw(_platformTexture, platform.Rectangle, Color.White);
			}

			foreach (var coin in _coins)
			{
				if (!coin.Collected)
				{
					var coinRectangle = new Rectangle((int)coin.Position.X, (int)coin.Position.Y, 20, 20); // Scale to 20x20
					_spriteBatch.Draw(_coinTexture, coinRectangle, Color.White);
				}
			}

			foreach (var enemy in _enemies)
			{
				_spriteBatch.Draw(_enemyTexture, enemy.Position, Color.White);
			}

			_spriteBatch.Draw(_playerTexture, _player.Position, Color.White);
			_spriteBatch.DrawString(_font, $"Score: {_score}", new Vector2(10, 10), Color.White);
			_spriteBatch.DrawString(_font, $"Time: {TimeLimit - _elapsedTime:F1} seconds", new Vector2(10, 40), Color.White);

		}

		private void DrawGameOverMenu()
		{


			// Draw a semi-transparent background
			_spriteBatch.Draw(_backgroundTexture, new Vector2(0, 0), new Color(0, 0, 0, 150));

			// Draw the game-over message
			_spriteBatch.DrawString(_font, "Game Over", new Vector2(250, 150), Color.Red);

			// Draw the score
			_spriteBatch.DrawString(_font, $"Score: {_score} coins collected", new Vector2(160, 250), Color.White);

			// Draw the elapsed time
			_spriteBatch.DrawString(_font, $"Time Elapsed: {_elapsedTime:F1} seconds", new Vector2(160, 300), Color.White);

			// Draw Restart button (as a texture with text)
			_spriteBatch.Draw(_platformTexture, new Vector2(250, 400), Color.DarkGray);
			_spriteBatch.DrawString(_font, "Restart", new Vector2(350, 415), Color.White);

			// Draw Exit button (as a texture with text)
			_spriteBatch.Draw(_platformTexture, new Vector2(250, 500), Color.DarkGray);
			_spriteBatch.DrawString(_font, "Exit", new Vector2(370, 515), Color.White);


		}
	}
}

