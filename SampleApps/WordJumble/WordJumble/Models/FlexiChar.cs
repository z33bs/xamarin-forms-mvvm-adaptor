using System;
using MvvmHelpers;
using Xamarin.Forms;

namespace WordJumble.Models
{
    public class FlexiChar
    {
        public FlexiChar(char character, int x, int y, int rotation, int size)
        {
            Character = character;
            PositionX = (double)x / Constants.GRID_COLS;
            PositionY = (double)y / Constants.GRID_ROWS;
            Rotation = rotation;
            FontSize = size;
        }

        public char Character { get; set; }

        public Rectangle LayoutRectangle => new Rectangle(PositionX, PositionY, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize);
        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public int Rotation { get; set; }

        public int FontSize { get; set; }
    }
}
