using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;

namespace WorldAtlas.Compatibility.GMCM
{
    internal class WideTextBox(int width, int height, string text = "") : IKeyboardSubscriber
    {
        private readonly Texture2D textBoxTexture = Game1.content.Load<Texture2D>("LooseSprites\\textBox");
        private readonly SpriteFont font = Game1.smallFont;

        public int X { get; set; }
        public int Y { get; set; }
        public int Height { get; set; } = height;
        public int Width { get; set; } = width;

        private bool _selected;

        public string Text { get; set; } = text;

        public bool Selected
        {
            get => _selected;
            set
            {
                if (_selected == value) return;

                _selected = value;
                if (_selected)
                {
                    Game1.keyboardDispatcher.Subscriber = this;
                }
                else
                {
                    if (Game1.keyboardDispatcher.Subscriber == this)
                        Game1.keyboardDispatcher.Subscriber = null;
                }
            }
        }

        /***********************
         * IKeyboardSubscriber *
         ***********************/
        public void RecieveCommandInput(char command)
        {
            if (!Selected) return;

            if (command == '\b' && Text.Length > 0)
            {
                Game1.playSound("tinyWhip");
                Text = Text[..^1];
            }
        }

        public void RecieveSpecialInput(Keys key) { }

        public void RecieveTextInput(char inputChar)
        {
            if (!Selected) return;

            switch (inputChar)
            {
                case '"':
                    return;
                case '$':
                    Game1.playSound("money");
                    break;
                case '*':
                    Game1.playSound("hammer");
                    break;
                case '+':
                    Game1.playSound("slimeHit");
                    break;
                case '<':
                    Game1.playSound("crystal");
                    break;
                case '=':
                    Game1.playSound("coin");
                    break;
                default:
                    Game1.playSound("cowboy_monsterhit");
                    break;
            }

            Text += inputChar;
        }

        public void RecieveTextInput(string text)
        {
            Text += text;
        }

        /******************
         * Draw and Click *
         ******************/
        public void Update(int x, int y)
        {
            ButtonState buttonState = Game1.input.GetMouseState().LeftButton;
            if (buttonState == ButtonState.Pressed)
            {
                int mouseX = Game1.getMouseX();
                int mouseY = Game1.getMouseY();

                Selected = x <= mouseX && mouseX <= x + Width
                        && y <= mouseY && mouseY <= y + Height;
            }
        }

        public void Draw(SpriteBatch b, Vector2 pos)
        {
            X = (Game1.uiViewport.Width - 16) / 2;
            Y = (int)pos.Y;
            Update(X, Y);

            // Caja
            b.Draw(textBoxTexture, new Rectangle(X, Y, 16, Height), new Rectangle(0, 0, 16, Height), Color.White);
            b.Draw(textBoxTexture, new Rectangle(X + 16, Y, Width - 32, Height), new Rectangle(16, 0, 4, Height), Color.White);
            b.Draw(textBoxTexture, new Rectangle(X + Width - 16, Y, 16, Height), new Rectangle(textBoxTexture.Bounds.Width - 16, 0, 16, Height), Color.White);

            // Recortar si es muy largo
            string tempText = Text;
            Vector2 vector = font.MeasureString(tempText);
            while (vector.X > Width)
            {
                tempText = tempText[1..];
                vector = font.MeasureString(tempText);
            }

            // Ni puta idea, revisarlo. Algo hace cada medio segundo
            bool flag = Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 1000.0 >= 500.0;
            if (flag && Selected)
            {
                b.Draw(Game1.staminaRect, new Rectangle(X + 16 + (int)vector.X + 2, Y + 8, 4, 32), Color.Black);
            }

            b.DrawString(font, tempText, new Vector2(X + 16, Y + 8), Color.Black, 0F, Vector2.Zero, 1f, SpriteEffects.None, 0.99f); //8 -> 12?
        }
    }
}
