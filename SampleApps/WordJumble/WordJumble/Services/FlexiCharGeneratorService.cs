using System;
using WordJumble.Models;

namespace WordJumble.Services
{
    public class FlexiCharGeneratorService
    {
        readonly Random random = new Random();

        public FlexiChar GetRandomFlexiChar(char character)
        {
            return new FlexiChar(
                character,
                x: random.Next(0, Constants.GRID_COLS),
                y: random.Next(0, Constants.GRID_ROWS),
                rotation: random.Next(Constants.ROTATION_MIN_ANGLE, Constants.ROTATION_MAX_ANGLE),
                size: random.Next(Constants.FONTSIZE_MIN, Constants.FONTSIZE_MAX));
        }
    }
}
