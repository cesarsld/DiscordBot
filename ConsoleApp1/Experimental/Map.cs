//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace BHungerGaemsBot
//{
//    class Map
//    {
//        private Coordinates[,] map = new Coordinates[30, 30];
//        public Dictionary<int, List<Coordinates>> dropAreas = new Dictionary<int, List<Coordinates>>();

//        public Map()
//        {
//            for (int i = 0; i < 8; i++)
//            {
//                dropAreas.Add(i, new List<Coordinates>());
//            }
//        }

//        public void InitMap()
//        {
//            for (int i = 0; i > map.GetLength(0); i++)
//            {
//                for (int j = 0; j > map.GetLength(1); j++)
//                {
//                    map[i, j] = new Coordinates(i, j);
//                }
//            }

//            BuildMediumLootZone();

//            BuildHighLootZone(5, 5);
//            BuildHighLootZone(5, 24);

//            BuildHighLootZone(24, 5);
//            BuildHighLootZone(24, 24);

//            BuildExtremeLootArea();

//            BuildDropableZone();
//        }

//        private void BuildMediumLootZone()
//        {
//            FillEvenPerimetre(4, 13, 2, 3, LootQuality.Medium);
//            FillEvenPerimetre(3, 12, 4, 5, LootQuality.Low);

//            FillEvenPerimetre(23, 13, 2, 3, LootQuality.Medium);
//            FillEvenPerimetre(22, 12, 4, 5, LootQuality.Low);

//            FillEvenPerimetre(14, 4, 3, 2, LootQuality.Medium);
//            FillEvenPerimetre(13, 2, 5, 4, LootQuality.Low);

//            FillEvenPerimetre(14, 23, 3, 2, LootQuality.Medium);
//            FillEvenPerimetre(13, 22, 5, 4, LootQuality.Low);


//        }

//        private void BuildHighLootZone(int x, int y)
//        {
//            map[x, y].lootQuality = LootQuality.High;
//            FillOutPerimetre(x, y, 3, 3, LootQuality.Medium);
//            FillOutPerimetre(x, y, 5, 5, LootQuality.Low);
//            FillOutPerimetre(x, y, 7, 7, LootQuality.Low);
//        }

//        private void BuildExtremeLootArea()
//        {
//            FillEvenPerimetre(14, 14, 2, 2, LootQuality.Extreme);
//            FillEvenPerimetre(13, 13, 4, 4, LootQuality.High);
//            FillEvenPerimetre(12, 12, 6, 6, LootQuality.Medium);
//            FillEvenPerimetreWater(11, 11, 8, 8);
//            FillEvenPerimetreWater(10, 10, 10, 10);
//        }

//        private void BuildDropableZone()
//        {
//            int dropZone = 0;
//            FillEvenPerimetreDropable(10, 3, 5, 2, dropZone);
//            dropZone++;
//            FillEvenPerimetreDropable(18, 3, 5, 2, dropZone);
//            dropZone++;

//            FillEvenPerimetreDropable(10, 22, 5, 2, dropZone);
//            dropZone++;
//            FillEvenPerimetreDropable(18, 22, 5, 2, dropZone);            dropZone++;
//            dropZone++;

//            FillEvenPerimetreDropable(3, 10, 2, 5, dropZone);
//            dropZone++;
//            FillEvenPerimetreDropable(3, 18, 2, 5, dropZone);
//            dropZone++;

//            FillEvenPerimetreDropable(22, 10, 2, 5, dropZone);
//            dropZone++;
//            FillEvenPerimetreDropable(22, 18, 2, 5, dropZone);
//        }

//        private void FillOutPerimetre (int x, int y, int length, int width, LootQuality quality)
//        {
//            int midLength = Convert.ToInt32(length / 2);
//            int midwitdh = Convert.ToInt32(width / 2);
//            for (int i = 0; i < width; i++)
//            {
//                map[x - midwitdh + i, y + midLength].lootQuality = quality;
//                map[x - midwitdh + i, y - midLength].lootQuality = quality;
//            }
//            for (int i = 0; i < length; i++)
//            {
//                map[x - midwitdh, y - midLength + i].lootQuality = quality;
//                map[x + midwitdh, y - midLength + i].lootQuality = quality;
//            }
//        }
//        private void FillEvenPerimetre(int x, int y, int length, int width, LootQuality quality)
//        {
//            for (int i = 0; i < length; i++)
//            {
//                map[x, y + i].lootQuality = quality;
//                map[x + width, y + i].lootQuality = quality;
//            }
//            for (int i = 0; i < width; i++)
//            {
//                map[x + i, y].lootQuality = quality;
//                map[x + i, y + length].lootQuality = quality;
//            }
//        }
//        private void FillEvenPerimetreWater(int x, int y, int length, int width)
//        {
//            for (int i = 0; i < length; i++)
//            {
//                map[x, y + i].isWater = true;
//                map[x + width, y + i].isWater = true;
//            }
//            for (int i = 0; i < width; i++)
//            {
//                map[x + i, y].isWater = true;
//                map[x + i, y + length].isWater = true;
//            }
//        }
//        private void FillEvenPerimetreDropable(int x, int y, int length, int width, int dropZone)
//        {
//            for (int i = 0; i < length; i++)
//            {
//                map[x, y + i].isDropable = true;
//                map[x, y + i].lootQuality = LootQuality.None;
//                dropAreas[dropZone].Add(map[x, y + i]);
//                map[x + width, y + i].isDropable = true;
//                map[x + width, y + i].lootQuality = LootQuality.None;
//                dropAreas[dropZone].Add(map[x + width, y + i]);
//            }
//            for (int i = 0; i < width; i++)
//            {
//                map[x + i, y].isDropable = true;
//                map[x + i, y].lootQuality = LootQuality.None;
//                dropAreas[dropZone].Add(map[x + i, y]);
//                map[x + i, y + length].isDropable = true;
//                map[x + i, y + length].lootQuality = LootQuality.None;
//                dropAreas[dropZone].Add(map[x + i, y + length]);
//            }
//        }
//    }
//}


///*
 
//     int midLength = Convert.ToInt32(length / 2);
//            int midwitdh = Convert.ToInt32(width / 2);
//            if (length % 2 == 1)
//            {
//                midLength++;
//            }
//            if (width % 2 == 1)
//            {
//                midwitdh++;
//            }

//     */
