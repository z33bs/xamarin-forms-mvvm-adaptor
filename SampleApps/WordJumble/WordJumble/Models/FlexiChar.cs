using System;
using MvvmHelpers;

namespace WordJumble.Models
{
    public class FlexiChar
    {
        public FlexiChar(char character, int x, int y, int rotation, int size)
        {
            Character = character;
            PositionX = x;
            PositionY = y;
            Rotation = rotation;
            FontSize = size;
        }

        public char Character { get; set; }

        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public int Rotation { get; set; }

        public int FontSize { get; set; }
    }
}
