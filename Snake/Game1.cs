using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Snake
{
    public struct Ball
    {
        public Vector2 _position;
    }

    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // ========================================
        #region Variables
        private Texture2D _backgroundTexture;
        private Texture2D _snakeTexture;
        private Texture2D _ballTexture;
        private Texture2D _rainbow;     // create variables used to store textures

        private List<Vector2> _snakeBodyList = new List<Vector2>(); // list of vecotrs, used to store "snake";

        private Random random = new Random();   // any randomers in chat

        private float _internalTimer = 0;

        private short _nextMove = 0; // 1 up, 2 down, 3 left, 4 right??
        private short _prevMove = 0;

        private int _score;

        private Ball _ball;

        private bool _dead;

        private SpriteFont _font;   // font.font


        #endregion
        // ========================================

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this)
            {
                PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8   // change stencil format, used later
            };
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _graphics.PreferredBackBufferWidth = 750;
            _graphics.PreferredBackBufferHeight = 800;  // specify screen size
            _graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            // Set Up Game
            SetUpGame();

            base.Initialize();
        }

        private void SetUpGame()    // method to initialise game
        {
            _snakeBodyList.Clear(); // remove all elements from snake body
            _snakeBodyList.Add(Vector2.Zero);   // add new body piece at 0,0
            _nextMove = 4;  // initially moves right
            _ball = new Ball(); // new ball pog
            _ball._position = MoveBall();   // move the ball
            _dead = false;
            _internalTimer = 0;
            _score = 0;
        }

        protected override void LoadContent()   // method to load content
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // load textures
            _backgroundTexture = Content.Load<Texture2D>("Background");
            _ballTexture = Content.Load<Texture2D>("Ball");
            _snakeTexture = Content.Load<Texture2D>("Snake");
            _font = Content.Load<SpriteFont>("Font");   // cheeky font
            _rainbow = Content.Load<Texture2D>("Rainbow");
        }

        protected override void Update(GameTime gameTime)
        {
            if (!_dead) // i live
            {
                // keyboard inputs
                {
                    if (Keyboard.GetState().IsKeyDown(Keys.Up) && !(_prevMove == 2 && _snakeBodyList.Count != 1))
                        _nextMove = 1;
                    else if (Keyboard.GetState().IsKeyDown(Keys.Down) && !(_prevMove == 1 && _snakeBodyList.Count != 1))
                        _nextMove = 2;
                    else if (Keyboard.GetState().IsKeyDown(Keys.Left) && !(_prevMove == 4 && _snakeBodyList.Count != 1))
                        _nextMove = 3;
                    else if (Keyboard.GetState().IsKeyDown(Keys.Right) && !(_prevMove == 3 && _snakeBodyList.Count != 1))
                        _nextMove = 4;  // get input, change move variable
                }

                if (_internalTimer == 0 || _internalTimer == 15 || _internalTimer == 30 || _internalTimer == 45) // move snake
                {
                    _prevMove = _nextMove;  // get previous move    
                    Vector2 _snakeTail = _snakeBodyList.Last(); // get position of tail
                    Vector2 _snakeHead = _snakeBodyList.First();    // get position of head
                    Vector2 _newHeadPosition = Vector2.Zero;    // new blank vector

                    switch (_nextMove)   // where move
                    {
                        case 1:
                            _newHeadPosition = Vector2.Add(_snakeHead, new Vector2(0, -50));    // get position of where head should move
                            break;
                        case 2:
                            _newHeadPosition = Vector2.Add(_snakeHead, new Vector2(0, 50));
                            break;
                        case 3:
                            _newHeadPosition = Vector2.Add(_snakeHead, new Vector2(-50, 0));
                            break;
                        case 4:
                            _newHeadPosition = Vector2.Add(_snakeHead, new Vector2(50, 0));
                            break;
                    }

                    // check if head is now out of bounds (death)
                    if (_newHeadPosition.X < 0 || _newHeadPosition.X > 700 || _newHeadPosition.Y < 0 || _newHeadPosition.Y > 700)
                    {
                        _dead = true;
                        // die
                    }

                    // check if snake has hit itself
                    else if (_snakeBodyList.Contains(_newHeadPosition))
                    {
                        _dead = true;
                        // die
                    }

                    // check if snake head touches apple
                    else if (_newHeadPosition == _ball._position)
                    {
                        _score++;   // increase score, move ball
                        _ball._position = MoveBall();
                        _snakeBodyList.Insert(0, _newHeadPosition);
                    }

                    else // regular movement
                    {
                        _snakeBodyList.Insert(0, _newHeadPosition); // add new "head" to snake
                        _snakeBodyList.RemoveAt(_snakeBodyList.Count - 1);  // if no ball hit, remove snake tail
                    }
                }

                _internalTimer = (_internalTimer + 1) % 60; // looping timer variable 0-59>0-59...
            }

            else // i die  
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Space))  // space to restart
                {
                    SetUpGame();
                }
            }

            base.Update(gameTime);
        }

        private Vector2 MoveBall() // moves ball to a new random position
        {
            while (true)    // loop
            {
                int x = random.Next(0, 15);
                int y = random.Next(0, 15); // grid coords

                Vector2 _newPos = new Vector2((float)x * 50, (float)y * 50);    // convert to vector
                if (_snakeBodyList.Contains(_newPos))   // if occupied by body
                    continue;   // reroll

                else // if not occupied by body
                    return _newPos; // return position
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);

            var m = Matrix.CreateOrthographicOffCenter(
                0, _graphics.PreferredBackBufferWidth,
                _graphics.PreferredBackBufferHeight,
                0, 0, 1);   // create matrix, orthographical, not perspective

            var a = new AlphaTestEffect(GraphicsDevice)
            {
                Projection = m, // new alpha test effect from orthographic matrix
            };  

            var s1 = new DepthStencilState  // create new depth stencil state:  // MASK
            {
                StencilEnable = true,   // it is enabled
                StencilFunction = CompareFunction.Always,   // ALWAYS passes
                StencilPass = StencilOperation.Replace, // when passing, replace pixel
                ReferenceStencil = 1,
                DepthBufferEnable = false,
            };

            var s2 = new DepthStencilState  // X TO MASK
            {
                StencilEnable = true,
                StencilFunction = CompareFunction.LessEqual, // if new pixel <= old, pass
                StencilPass = StencilOperation.Keep,    // if pass, keep OLD pixel
                ReferenceStencil = 1,
                DepthBufferEnable = false,
            };

            _spriteBatch.Begin(SpriteSortMode.Immediate, null, null, s1, null, a);  // new spritebatch, use s1 (MASK)
            foreach (Vector2 _position in _snakeBodyList)
            {
                _spriteBatch.Draw(_snakeTexture, _position, Color.White);   // draw each section of snake's body
            }
            _spriteBatch.End(); // end batch

            _spriteBatch.Begin(SpriteSortMode.Immediate, null, null, s2, null, a);  // new sb, use s2 (IMAGE TO MASK)
            _spriteBatch.Draw(_rainbow, new Vector2((float)75 / 6 * _internalTimer, 0), Color.White);   // draw rainbow with offset
            _spriteBatch.Draw(_rainbow, new Vector2((float)75 / 6 * _internalTimer - 750, 0), Color.White); // draw raibow with offset to repeat
            _spriteBatch.End();

            _spriteBatch.Begin();   // new spritebatch pog?

            // Draw Grid
            _spriteBatch.Draw(_backgroundTexture, Vector2.Zero, Color.White);

            // Draw Ball
            _spriteBatch.Draw(_ballTexture, _ball._position, Color.Red);

            // Draw Score
            _spriteBatch.DrawString(_font, "Score: " + _score.ToString(), new Vector2(50, 760), Color.Black);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
