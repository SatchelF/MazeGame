using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MazeGame
{
    public class Game1 : Game
    {
        // Graphics and drawing fields
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private Texture2D backgroundTexture; // Background texture for the maze area
        private Texture2D pixelTexture; // 1x1 pixel texture for drawing lines
        private Texture2D playerTexture; // Player texture
        private SpriteFont titleFont;
        private SpriteFont buttonFont;
        private Texture2D breadcrumbTexture; // Breadcrumb texture
        private bool showBreadcrumbs = false; // Flag to toggle breadcrumbs visibility
        private bool showHint = false; // Tracks hint visibility


        // Display and gameplay settings
        private List<Point> breadcrumbs = new List<Point>(); // List to track visited cells
        private const int MazeDisplayWidth = 525;
        private const int MazeDisplayHeight = 525;
        private int cellSize;

        // Game state management
        private KeyboardState previousKeyboardState;
        private GameState currentState;
        public enum GameState { MainMenu, Playing}
        private bool showHighScores = false;
        private bool showCredits = false;
        private Dictionary<int, int> highScoresBySize = new Dictionary<int, int>();


        // Timing and scoring
        private TimeSpan gameTimer;
        private int gameScore = 0;
        private int[] highScores = new int[3]; // Array to store top 3 high scores
        private bool gameActive;

        // Maze and player
        private MazeGenerator mazeGenerator;
        private Point playerGridPosition;
        private Point endPoint;
        private Vector2 playerScreenPosition;
        private List<Point> shortestPath = new List<Point>();
        private bool showShortestPath = false;
        private Stack<Point> shortestPathStack = new Stack<Point>();

        public Game1()
        {

            // Initialize high scores for each maze size
            highScoresBySize.Add(5, 0);  // For 5x5 maze
            highScoresBySize.Add(10, 0); // For 10x10 maze
            highScoresBySize.Add(15, 0); // For 15x15 maze
            highScoresBySize.Add(20, 0); // For 20x20 maze

            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

        }

        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = 1000;
            graphics.PreferredBackBufferHeight = 800;
            graphics.ApplyChanges();
            currentState = GameState.MainMenu;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            titleFont = Content.Load<SpriteFont>("Title");
            buttonFont = Content.Load<SpriteFont>("button");
            breadcrumbTexture = Content.Load<Texture2D>("breadcrumb");

            // Create and set a 1x1 white pixel texture
            pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
            pixelTexture.SetData(new[] { Color.White });

            // Load the background texture
            backgroundTexture = Content.Load<Texture2D>("maze-background2");

            // Load the player texture
            playerTexture = Content.Load<Texture2D>("maze-player");
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            ProcessInput();

            if (currentState == GameState.Playing && gameActive)
            {
                if (playerGridPosition != endPoint)
                {
                    gameTimer += gameTime.ElapsedGameTime;
                }
                else
                {
                    gameActive = false;
                }
            }
        }


        private void ProcessInput()
        {
            KeyboardState currentKeyboardState = Keyboard.GetState();

            // Toggle visibility of breadcrumbs
            if (currentKeyboardState.IsKeyDown(Keys.B) && previousKeyboardState.IsKeyUp(Keys.B))
            {
                showBreadcrumbs = !showBreadcrumbs;
            }

            // Toggle visibility of the shortest path
            if (currentKeyboardState.IsKeyDown(Keys.P) && previousKeyboardState.IsKeyUp(Keys.P))
            {
                showShortestPath = !showShortestPath;
                if (showShortestPath || showHint)
                {
                    UpdateShortestPath();
                }
            }

            // Toggle hint visibility
            if (currentKeyboardState.IsKeyDown(Keys.H) && previousKeyboardState.IsKeyUp(Keys.H))
            {
                showHint = !showHint;
                UpdateShortestPath();
            }

            // Toggle visibility of high scores
            if (currentKeyboardState.IsKeyDown(Keys.F5) && previousKeyboardState.IsKeyUp(Keys.F5))
            {
                showHighScores = !showHighScores;
            }

            // Toggle visibility of credits
            if (currentKeyboardState.IsKeyDown(Keys.F6) && previousKeyboardState.IsKeyUp(Keys.F6))
            {
                showCredits = !showCredits;
            }

            // Game controls
            if (currentState == GameState.Playing)
            {
                MovePlayerBasedOnInput(currentKeyboardState);
            }

            // Starting a new game
            if (currentKeyboardState.IsKeyDown(Keys.F1) && previousKeyboardState.IsKeyUp(Keys.F1)) StartGame(5);
            else if (currentKeyboardState.IsKeyDown(Keys.F2) && previousKeyboardState.IsKeyUp(Keys.F2)) StartGame(10);
            else if (currentKeyboardState.IsKeyDown(Keys.F3) && previousKeyboardState.IsKeyUp(Keys.F3)) StartGame(15);
            else if (currentKeyboardState.IsKeyDown(Keys.F4) && previousKeyboardState.IsKeyUp(Keys.F4)) StartGame(20);

            previousKeyboardState = currentKeyboardState;
        }

        private void MovePlayerBasedOnInput(KeyboardState currentKeyboardState)
        {
            if (currentState == GameState.Playing)
            {
                // Up movement - Arrow Up, W, or I
                if ((currentKeyboardState.IsKeyDown(Keys.Up) || currentKeyboardState.IsKeyDown(Keys.W) || currentKeyboardState.IsKeyDown(Keys.I)) && previousKeyboardState.IsKeyUp(Keys.Up) && previousKeyboardState.IsKeyUp(Keys.W) && previousKeyboardState.IsKeyUp(Keys.I))
                    MovePlayer("n");

                // Down movement - Arrow Down, S, or K
                if ((currentKeyboardState.IsKeyDown(Keys.Down) || currentKeyboardState.IsKeyDown(Keys.S) || currentKeyboardState.IsKeyDown(Keys.K)) && previousKeyboardState.IsKeyUp(Keys.Down) && previousKeyboardState.IsKeyUp(Keys.S) && previousKeyboardState.IsKeyUp(Keys.K))
                    MovePlayer("s");

                // Left movement - Arrow Left, A, or J
                if ((currentKeyboardState.IsKeyDown(Keys.Left) || currentKeyboardState.IsKeyDown(Keys.A) || currentKeyboardState.IsKeyDown(Keys.J)) && previousKeyboardState.IsKeyUp(Keys.Left) && previousKeyboardState.IsKeyUp(Keys.A) && previousKeyboardState.IsKeyUp(Keys.J))
                    MovePlayer("w");

                // Right movement - Arrow Right, D, or L
                if ((currentKeyboardState.IsKeyDown(Keys.Right) || currentKeyboardState.IsKeyDown(Keys.D) || currentKeyboardState.IsKeyDown(Keys.L)) && previousKeyboardState.IsKeyUp(Keys.Right) && previousKeyboardState.IsKeyUp(Keys.D) && previousKeyboardState.IsKeyUp(Keys.L))
                    MovePlayer("e");
            }
        }

        private void MovePlayer(string direction)
        {
            // Retrieve the current cell that the player is in using their grid position.
            Cell currentCell = mazeGenerator.grid[playerGridPosition.X, playerGridPosition.Y];
            UpdateScore(playerGridPosition);
            if (currentCell.Edges.ContainsKey(direction) && currentCell.Edges[direction] != null)
            {
                playerGridPosition = new Point(currentCell.Edges[direction].X, currentCell.Edges[direction].Y);

                // After updating the player's grid position, add the new position to the breadcrumbs list
                if (!breadcrumbs.Contains(playerGridPosition)) // Check if the new position is not already in the breadcrumbs
                {
                    breadcrumbs.Add(playerGridPosition); // Add the new position to the breadcrumbs list
                }

                if (showShortestPath || showHint)
                {
                    // If moving off the path, recalculate and update the stack
                    if (!shortestPathStack.Peek().Equals(playerGridPosition))
                    {
                        UpdateShortestPath();
                    }
                    else
                    {
                        shortestPathStack.Pop();
                    }
                }

                UpdatePlayerScreenPosition(); // Update the player's screen position
            }
        }




        private void StartGame(int size)
        {
            currentState = GameState.Playing;
            mazeGenerator = new MazeGenerator(size, size);
            mazeGenerator.GenerateMaze();

            cellSize = Math.Min(MazeDisplayWidth / size, MazeDisplayHeight / size);
            playerGridPosition = new Point(0, 0); // Start at the top-left corner of the maze.
            endPoint = new Point(size - 1, size - 1); // End at the bottom-right corner.
            gameActive = true; // Game is now active.
            gameTimer = TimeSpan.Zero; // Reset game timer.

            breadcrumbs.Clear(); // Clear any existing breadcrumbs from previous games.
            breadcrumbs.Add(playerGridPosition); 
            gameScore = 0; // Reset score at the start of the game
            showShortestPath = true;
            UpdateShortestPath();
            showHint = false; 
            showShortestPath = false;

            UpdatePlayerScreenPosition();
        }


        private void UpdatePlayerScreenPosition()
        {
            // Calculate the offset to center the maze on the screen
            Vector2 mazeOffset = new Vector2((graphics.PreferredBackBufferWidth - MazeDisplayWidth) / 2, (graphics.PreferredBackBufferHeight - MazeDisplayHeight) / 2);
            playerScreenPosition = new Vector2(playerGridPosition.X * cellSize + mazeOffset.X, playerGridPosition.Y * cellSize + mazeOffset.Y);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();

            string[] legendLines = {
                "B: Toggle Breadcrumbs",
                "P: Toggle Shortest Path",
                "H: Toggle Hint"
            };
            Vector2 legendPosition = new Vector2(graphics.PreferredBackBufferWidth - 250, 700); // Adjust for your layout
            foreach (string line in legendLines)
            {
                spriteBatch.DrawString(buttonFont, line, legendPosition, Color.White);
                legendPosition.Y += 20; 
            }

            // Draw the game title at the top center
            Vector2 titleSize = titleFont.MeasureString("The Maze Game");
                spriteBatch.DrawString(titleFont, "The Maze Game", new Vector2((graphics.PreferredBackBufferWidth - titleSize.X) / 2, 20), Color.GhostWhite);

                // Draw the current score
                spriteBatch.DrawString(buttonFont, $"Score: {gameScore}", new Vector2(20, 40), Color.White);

                // Draw menu choices on the left if in MainMenu or Playing state
                DrawMenuChoices();

                // Draw the maze
                DrawMaze();

                // Draw the player
                DrawPlayer();

                // Draw the high score and timer
                
                DrawTimer();

                // Draw breadcrumbs if toggled on
                if (showBreadcrumbs)
                {
                    foreach (var breadcrumb in breadcrumbs)
                    {
                        Vector2 breadcrumbPosition = new Vector2(breadcrumb.X * cellSize, breadcrumb.Y * cellSize) + new Vector2((graphics.PreferredBackBufferWidth - MazeDisplayWidth) / 2, (graphics.PreferredBackBufferHeight - MazeDisplayHeight) / 2);
                        spriteBatch.Draw(breadcrumbTexture, new Rectangle((int)breadcrumbPosition.X, (int)breadcrumbPosition.Y, cellSize, cellSize), Color.LightYellow);
                    }
                }

            if (showCredits)
            {
                string creditsText = "Credits:\nSatchel Fausett\nsatchcollege@gmail.com";
                // Calculate position based on screen size and text size for centered alignment
                Vector2 creditsSize = buttonFont.MeasureString(creditsText);
                Vector2 creditsPosition = new Vector2((graphics.PreferredBackBufferWidth - creditsSize.X) / 2, graphics.PreferredBackBufferHeight - creditsSize.Y - 20);
                spriteBatch.DrawString(buttonFont, creditsText, creditsPosition, Color.White);
            }

            if (showHighScores)
            {
                int i = 0; 
                foreach (var mazeSize in highScoresBySize.Keys.OrderBy(k => k))
                {
                    string highScoreText = $"High Score {mazeSize}x{mazeSize}: {highScoresBySize[mazeSize]}";
                    spriteBatch.DrawString(buttonFont, highScoreText, new Vector2(20, graphics.PreferredBackBufferHeight - 120 + (i * 20)), Color.Goldenrod);
                    i++;
                }
            }


            // Draw shortest path if toggled on
            if (showShortestPath && shortestPathStack.Count > 0)
                {
                    var pathArray = shortestPathStack.ToArray(); // Convert stack to array for drawing
                    for (int i = pathArray.Length - 1; i >= 0; i--) // Draw from the start to the end point
                    {
                        Vector2 pathPosition = new Vector2(pathArray[i].X * cellSize, pathArray[i].Y * cellSize) + new Vector2((graphics.PreferredBackBufferWidth - MazeDisplayWidth) / 2, (graphics.PreferredBackBufferHeight - MazeDisplayHeight) / 2);
                        spriteBatch.Draw(breadcrumbTexture, new Rectangle((int)pathPosition.X, (int)pathPosition.Y, cellSize, cellSize), Color.Red);
                    }
                }

                // Draw hint if toggled on and there are steps to follow
                if (showHint && shortestPathStack.Count > 0)
                {
                    if (shortestPathStack.Peek().Equals(playerGridPosition) && shortestPathStack.Count > 1)
                    {
                        shortestPathStack.Pop(); // Remove the current position to access the next step.
                        var hintPoint = shortestPathStack.Peek(); // Peek the true next move.
                        DrawHint(hintPoint); // Draw the hint with the corrected next step.
                        shortestPathStack.Push(playerGridPosition); // Restore the current position back to the stack.
                    }
                    else if (!shortestPathStack.Peek().Equals(playerGridPosition))
                    {
                        var hintPoint = shortestPathStack.Peek(); // If the top doesn't match the player's position, it's already the next move.
                        DrawHint(hintPoint);
                    }
                }
       
            spriteBatch.End();
            base.Draw(gameTime);
        }


        private void DrawHint(Point nextMove)
        {
            Vector2 hintPosition = new Vector2(nextMove.X * cellSize, nextMove.Y * cellSize) +
                                   new Vector2((graphics.PreferredBackBufferWidth - MazeDisplayWidth) / 2,
                                               (graphics.PreferredBackBufferHeight - MazeDisplayHeight) / 2);

          
            spriteBatch.Draw(pixelTexture,
                             new Rectangle((int)hintPosition.X, (int)hintPosition.Y, cellSize, cellSize),
                             Color.Goldenrod); 
        }



        private void DrawMenuChoices()
        {
            float menuStartY = 100;
            float menuItemSpacing = 60;

            spriteBatch.DrawString(buttonFont, "F1 - New Game 5x5", new Vector2(10, menuStartY), Color.GhostWhite);
            spriteBatch.DrawString(buttonFont, "F2 - New Game 10x10", new Vector2(10, menuStartY + menuItemSpacing), Color.GhostWhite);
            spriteBatch.DrawString(buttonFont, "F3 - New Game 15x15", new Vector2(10, menuStartY + menuItemSpacing * 2), Color.GhostWhite);
            spriteBatch.DrawString(buttonFont, "F4 - New Game 20x20", new Vector2(10, menuStartY + menuItemSpacing * 3), Color.GhostWhite);
            spriteBatch.DrawString(buttonFont, "F5 - High Scores", new Vector2(10, menuStartY + menuItemSpacing * 4), Color.GhostWhite);
            spriteBatch.DrawString(buttonFont, "F6 - Credits", new Vector2(10, menuStartY + menuItemSpacing * 5), Color.GhostWhite);
        }

        private void DrawMaze()
        {
            // Early exit if maze generator is not initialized
            if (mazeGenerator == null) return;

            // Setup for drawing the maze
            int mazeWidth = cellSize * mazeGenerator.width;
            int mazeHeight = cellSize * mazeGenerator.height;
            Vector2 mazePosition = new Vector2(
                (graphics.PreferredBackBufferWidth - mazeWidth) / 2,
                (graphics.PreferredBackBufferHeight - mazeHeight) / 2
            );

            // Drawing maze background
            spriteBatch.Draw(
                backgroundTexture,
                new Rectangle((int)mazePosition.X, (int)mazePosition.Y, mazeWidth, mazeHeight),
                Color.White
            );

            int lineWidth = 3;

            // Drawing walls within the maze
            for (int y = 0; y < mazeGenerator.height; y++)
            {
                for (int x = 0; x < mazeGenerator.width; x++)
                {
                    DrawWalls(x, y, mazePosition, lineWidth);
                }
            }

            // Draw start and end indicators
            DrawIndicator(mazePosition, Color.LightGreen, 0, 0); // Start indicator
            DrawIndicator(mazePosition, Color.IndianRed, endPoint.X, endPoint.Y); // End indicator
        }

        private void DrawWalls(int x, int y, Vector2 mazePosition, int lineWidth)
        {
            Cell cell = mazeGenerator.grid[x, y];
            Vector2 cellPosition = new Vector2(x * cellSize, y * cellSize) + mazePosition;

            // North wall (drawn for the first row of cells)
            if (y == 0)
            {
                spriteBatch.Draw(
                    pixelTexture,
                    new Rectangle((int)cellPosition.X, (int)cellPosition.Y - (lineWidth / 2), cellSize, lineWidth),
                    null, Color.White
                );
            }

            // West wall (drawn for the first column of cells)
            if (x == 0)
            {
                spriteBatch.Draw(
                    pixelTexture,
                    new Rectangle((int)cellPosition.X - (lineWidth / 2), (int)cellPosition.Y, lineWidth, cellSize),
                    null, Color.White
                );
            }

            // East wall
            if (cell.Edges["e"] == null || !cell.Edges["e"].Visited)
            {
                spriteBatch.Draw(
                    pixelTexture,
                    new Rectangle((int)cellPosition.X + cellSize - (lineWidth / 2), (int)cellPosition.Y, lineWidth, cellSize),
                    null, Color.White
                );
            }

            // South wall
            if (cell.Edges["s"] == null || !cell.Edges["s"].Visited)
            {
                spriteBatch.Draw(
                    pixelTexture,
                    new Rectangle((int)cellPosition.X, (int)cellPosition.Y + cellSize - (lineWidth / 2), cellSize, lineWidth),
                    null, Color.White
                );
            }
        }


        private void DrawIndicator(Vector2 mazePosition, Color color, int cellX, int cellY)
        {
            float scale = 0.8f;
            int squareSize = (int)(cellSize * scale);
            int offset = (cellSize - squareSize) / 2; // Center the square within the cell

            Vector2 squarePosition = new Vector2(
                mazePosition.X + (cellX * cellSize) + offset,
                mazePosition.Y + (cellY * cellSize) + offset
            );

            spriteBatch.Draw(
                pixelTexture,
                new Rectangle((int)squarePosition.X, (int)squarePosition.Y, squareSize, squareSize),
                color
            );
        }



        private void DrawPlayer()
        {
            // Calculate the center of the player circle
            float circleCenterX = playerScreenPosition.X + cellSize / 2 + 10;
            float circleCenterY = playerScreenPosition.Y + cellSize / 2 + 10;

            // Calculate the radius of the player circle (half of the cell size plus additional size)
            float circleRadius = (cellSize / 2) + 80; 

            // Draw the player circle
            spriteBatch.Draw(playerTexture, new Vector2(circleCenterX, circleCenterY), null, Color.White, 0f,
                             new Vector2(playerTexture.Width / 2, playerTexture.Height / 2), circleRadius / (playerTexture.Width / 2),
                             SpriteEffects.None, 0f);
        }

        private void DrawTimer()
        {
            // Draw timer above the maze
            string timerText = "Time: " + gameTimer.TotalSeconds.ToString("F1") + "s";
            Vector2 timerSize = buttonFont.MeasureString(timerText);
            spriteBatch.DrawString(buttonFont, timerText, new Vector2((graphics.PreferredBackBufferWidth - timerSize.X) / 2, 70), Color.GhostWhite);
        }

        private void UpdateShortestPath()
        {
            if (showShortestPath || showHint)
            {
                shortestPath = mazeGenerator.FindPath(playerGridPosition, endPoint); // Calculate the shortest path
                shortestPathStack.Clear(); // Clear existing stack
                                           // Reverse the path to push onto the stack so the next step is always at the top
                foreach (var step in shortestPath.AsEnumerable().Reverse())
                {
                    shortestPathStack.Push(step);
                }
            }
        }



        private void UpdateScore(Point playerPosition)
        {
            var cell = mazeGenerator.grid[playerPosition.X, playerPosition.Y];
            var pathAsList = shortestPathStack.ToList();

            if (!cell.PlayerVisited)
            {
                bool isOnShortestPath = pathAsList.Contains(playerPosition);

                if (isOnShortestPath)
                {
                    gameScore += 5;
                }
                else
                {
                    gameScore -= 2;
                }

                cell.PlayerVisited = true;
            }

            // Update high score for the current maze size
            int currentMazeSize = mazeGenerator.width; // Assuming width and height are the same
            if (gameScore > highScoresBySize[currentMazeSize])
            {
                highScoresBySize[currentMazeSize] = gameScore;
            }
        }








    }
}
