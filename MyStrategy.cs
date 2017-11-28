using System;
using System.Collections.Generic;
using Com.CodeGame.CodeWars2017.DevKit.CSharpCgdk.Model;

namespace Com.CodeGame.CodeWars2017.DevKit.CSharpCgdk
{
    public sealed class MyStrategy : IStrategy
    {

        private Random random;

        private static Dictionary<VehicleType, VehicleType[]> preferredTargetTypesByVehicleType = new Dictionary<VehicleType, VehicleType[]>() {
            {VehicleType.Fighter, 
                new VehicleType[] {VehicleType.Helicopter, VehicleType.Fighter}},
            {VehicleType.Helicopter, 
                new VehicleType[] {VehicleType.Tank, VehicleType.Arrv, VehicleType.Helicopter, VehicleType.Ifv, VehicleType.Fighter}},
            {VehicleType.Ifv, 
                new VehicleType[] {VehicleType.Helicopter, VehicleType.Arrv, VehicleType.Ifv, VehicleType.Fighter, VehicleType.Tank}},
            {VehicleType.Tank, 
                new VehicleType[] {VehicleType.Ifv, VehicleType.Arrv, VehicleType.Tank, VehicleType.Fighter, VehicleType.Helicopter}}
        };
        private TerrainType[][] terrainTypeByCellXY;
        private WeatherType[][] weatherTypeByCellXY;

        private Player me;
        private World world;
        private Game game;
        private Move move;

        private Dictionary<int, Vehicle> vehicleById = new Dictionary<int, Vehicle>();
        private Dictionary<int, int> updateTickByVehicleId = new Dictionary<int, int>();
        private Dictionary<int, Vehicle> Queue = new Dictionary<int, Vehicle>();

        public void Move(Player me, World world, Game game, Move move)
        {
            initializeStrategy(world, game);
            initializeTick(me, world, game, move);

            if (me.RemainingActionCooldownTicks > 0)
            {
                return;
            }

            if (executeDelayedMove())
            {
                return;
            }

            move();

            executeDelayedMove();
        }

        /**
         * Инциализируем стратегию.
         * <p>
         * Для этих целей обычно можно использовать конструктор, однако в данном случае мы хотим инициализировать генератор
         * случайных чисел значением, полученным от симулятора игры.
         */
        private void initializeStrategy(World world, Game game)
        {
            if (random == null)
            {
                random = new Random(game.getRandomSeed());

                terrainTypeByCellXY = world.TerrainByCellXY();
                weatherTypeByCellXY = world.WeatherByCellXY();
            }
        }

        /**
         * Сохраняем все входные данные в полях класса для упрощения доступа к ним, а также актуализируем сведения о каждой
         * технике и времени последнего изменения её состояния.
         */
        private void initializeTick(Player me, World world, Game game, Move move)
        {
            this.me = me;
            this.world = world;
            this.game = game;
            this.move = move;

            for (Vehicle vehicle : world.getNewVehicles())
            {
                vehicleById.put(vehicle.getId(), vehicle);
                updateTickByVehicleId.put(vehicle.getId(), world.getTickIndex());
            }

            for (VehicleUpdate vehicleUpdate : world.getVehicleUpdates())
            {
                long vehicleId = vehicleUpdate.getId();

                if (vehicleUpdate.getDurability() == 0)
                {
                    vehicleById.remove(vehicleId);
                    updateTickByVehicleId.remove(vehicleId);
                }
                else
                {
                    vehicleById.put(vehicleId, new Vehicle(vehicleById.get(vehicleId), vehicleUpdate));
                    updateTickByVehicleId.put(vehicleId, world.getTickIndex());
                }
            }
        }

        private enum Ownership
        {
            ANY,

            ALLY,

            ENEMY
        }


    }
}