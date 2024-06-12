using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KingdomInvader
{
    // The combat area will handle meele engagment, any range units will have no self defense ability
    // but can engange in any CombatArea or enemy squad if it is in range
    // The purpose for the combat area is to deny movment of the enemy squads and animate the actual combat
    public partial class Combat : Node2D
    {
        public List<Squad> Squads { get; set; } = new List<Squad>();
        // In case a squad joins at a later time after the combat has started
        // Will modify the outcome of JoinCombat
        public bool CombatHasStarted { get; private set; } = false;

        private Timer combatTimer;
        private Vector2 startPosition;

        // Handle the combat between N squads
        // keep reducing the population of the squads until only one squad is left
        public override void _Ready()
        {
            // TODO make sure squad always is of length 2 or more and includes at least two factions that are not allied
            GD.PushError("Squad started melee Combat");
            foreach (var squad in Squads)
            {
                JoinCombat(squad);
            }

            // Reduce the population of both squads by one every second until one squad disappears
            combatTimer = new Timer();
            combatTimer.WaitTime = 1.0f;
            combatTimer.OneShot = false;
            AddChild(combatTimer);
            combatTimer.Start();
            combatTimer.Connect("timeout", Callable.From(() => OnCombatTick()));
            CombatHasStarted = true;

            startPosition = Squads[0].Position;
        }

        public override void _Draw()
        {
            float innerRadius = 40;
            float outerRadius = 70;
            float angleFrom = 0;
            float angleTo = 360;
            var color = new Color(1, 0, 0);
            DrawArc(startPosition, innerRadius, angleFrom, angleTo, 360, color);
            DrawArc(startPosition, outerRadius, angleFrom, angleTo, 360, color);
        }

        public void JoinCombat(Squad squad)
        {
            // Squad has joined the combat after inital start
            if (CombatHasStarted)
            {
                GD.PushError("Squad entered melee Combat");
                Squads.Add(squad);
            }
            // Stop squads from moving
            squad.Destination = squad.Position + squad.Size / 2;
            squad.MovementBlocked = true;
            squad.InvolvedCombat = this;
        }

        private Squad FindRandomEnemySquad(Squad squad)
        {
            // Shuffle the Squads list
            var random = new Random();
            var shuffledSquads = Squads.OrderBy(x => random.Next()).ToList();

            // Find the first enemy squad
            return shuffledSquads.FirstOrDefault(s => s.PlayerOwner != squad.PlayerOwner);
        }

        private void ResolveCombat()
        {
            combatTimer.Stop();
            combatTimer = null;
            foreach (var squad in Squads)
            {
                squad.MovementBlocked = false;
                squad.InvolvedCombat = null;
            }
            QueueFree();
        }

        // Each squad will search for an enemy squad and attack it
        // TODO this needs some sort of damage multiplier based on the size of the squad for balance
        // TODO every squad should do something every now and then, mergind with other bigger squads if they go below a certain size
        // squads should move around and act like they are fighting
        public void OnCombatTick()
        {
            var squadToBeCleared = new List<Squad>();
            var playersHadTurn = new List<Player>();// for now we use this so per tick everyone does dmg once

            foreach (var squad in Squads)
            {
                GD.PushError(squad, Squads);
                if (squad == null || squad.Population <= 0)
                {
                    continue;
                }
                if (playersHadTurn.FirstOrDefault(s => s == squad.PlayerOwner) == null)
                {
                    playersHadTurn.Add(squad.PlayerOwner);
                    var target = FindRandomEnemySquad(squad);
                    if (target != null)
                    {
                        target.Population--;
                        // target may equal null if we have no more enemies
                        if (target.Population <= 0)
                        {
                            //target.QueueFree();
                            RemoveChild(target);
                        }
                    }

                    Random random = new Random();
                    float angle = random.Next(0, 360);
                    // Calculate the angle for the squad to move on the inner arc
                    //float angle = (float)(Math.Atan2(squad.Position.Y - startPosition.Y, squad.Position.X - startPosition.X) * 180 / Math.PI);
                    //angle = angle < 0 ? angle + 360 : angle;
                    float innerradius = 40;

                    // Calculate the new position for the squad on the inner arc
                    float x = startPosition.X + (float)(Math.Cos(angle * Math.PI / 180) * innerradius);
                    float y = startPosition.Y + (float)(Math.Sin(angle * Math.PI / 180) * innerradius);
                    squad.Destination = new Vector2(x, y);
                }
            }

            // TODO or if no other player involved
            if (Squads.Count <= 1 || FindRandomEnemySquad(Squads[0]) == null)
            {
                ResolveCombat();
            }
        }
    }
}
