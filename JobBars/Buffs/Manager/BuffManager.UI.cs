using Dalamud.Bindings.ImGui;
using JobBars.Data;
using System.Numerics;

namespace JobBars.Buffs.Manager {
    public partial class BuffManager {
        public bool LOCKED = true;

        private readonly InfoBox<BuffManager> PositionInfoBox = new() {
            Label = "Position",
            ContentsAction = manager => {
                ImGui.Checkbox( Loc.T( "Position locked" ) + manager.Id, ref manager.LOCKED );

                ImGui.SetNextItemWidth( 25f );
                if( ImGui.InputInt( Loc.T( "Buffs per line" ) + manager.Id, ref JobBars.Configuration.BuffHorizontal, 0 ) ) {
                    JobBars.Configuration.Save();
                }

                if( ImGui.Checkbox( Loc.T( "Right-to-left" ) + manager.Id, ref JobBars.Configuration.BuffRightToLeft ) ) {
                    JobBars.Configuration.Save();
                }

                if( ImGui.Checkbox( Loc.T( "Bottom-to-top" ) + manager.Id, ref JobBars.Configuration.BuffBottomToTop ) ) {
                    JobBars.Configuration.Save();
                }

                if( ImGui.Checkbox( Loc.T( "Square buffs" ) + manager.Id, ref JobBars.Configuration.BuffSquare ) ) {
                    JobBars.Configuration.Save();
                }

                if( ImGui.InputFloat( Loc.T( "Scale" ) + manager.Id, ref JobBars.Configuration.BuffScale ) ) {
                    JobBars.Configuration.Save();
                }

                var pos = JobBars.Configuration.BuffPosition;
                if( ImGui.InputFloat2( Loc.T( "Position" ) + manager.Id, ref pos ) ) {
                    SetBuffPosition( pos );
                }
            }
        };

        private readonly InfoBox<BuffManager> HideWhenInfoBox = new() {
            Label = "Hide When",
            ContentsAction = manager => {
                if( ImGui.Checkbox( Loc.T( "Out of combat" ), ref JobBars.Configuration.BuffHideOutOfCombat ) ) JobBars.Configuration.Save();
                if( ImGui.Checkbox( Loc.T( "Weapon is sheathed" ), ref JobBars.Configuration.BuffHideWeaponSheathed ) ) JobBars.Configuration.Save();
            }
        };

        protected override void DrawHeader() {
            if( ImGui.Checkbox( Loc.T( "Buff bar enabled" ) + Id, ref JobBars.Configuration.BuffBarEnabled ) ) {
                JobBars.Configuration.Save();
                ResetUi();
            }
        }

        protected override void DrawSettings() {
            PositionInfoBox.Draw( this );
            HideWhenInfoBox.Draw( this );

            ImGui.SetNextItemWidth( 50f );
            if( ImGui.InputFloat( Loc.T( "Hide buffs with cooldown above" ) + Id, ref JobBars.Configuration.BuffDisplayTimer ) ) JobBars.Configuration.Save();

            if( ImGui.Checkbox( Loc.T( "Show party members' buffs" ), ref JobBars.Configuration.BuffIncludeParty ) ) {
                JobBars.Configuration.Save();
                ResetUi();
            }

            if( ImGui.Checkbox( Loc.T( "Thin buff border" ), ref JobBars.Configuration.BuffThinBorder ) ) {
                JobBars.Configuration.Save();
            }

            ImGui.SetNextItemWidth( 50f );
            if( ImGui.InputFloat( Loc.T( "Opacity when on cooldown" ) + Id, ref JobBars.Configuration.BuffOnCDOpacity ) ) JobBars.Configuration.Save();

            ImGui.SetNextItemWidth( 100f );
            if( ImGui.InputInt( Loc.T( "Buff text size" ), ref JobBars.Configuration.BuffTextSize_v2 ) ) {
                if( JobBars.Configuration.BuffTextSize_v2 <= 0 ) JobBars.Configuration.BuffTextSize_v2 = 1;
                if( JobBars.Configuration.BuffTextSize_v2 > 255 ) JobBars.Configuration.BuffTextSize_v2 = 255;
                JobBars.Configuration.Save();
            }
        }

        protected override void DrawItem( BuffConfig[] item, JobIds _ ) {
            var reset = false;
            foreach( var buff in item ) buff.Draw( Id, ref reset );
            if( reset ) ResetUi();
        }

        public void DrawPositionBox() {
            if( LOCKED ) return;
            if( JobBars.DrawPositionView( BuffPositionId, JobBars.Configuration.BuffPosition, out var pos ) ) SetBuffPosition( pos );
        }

        internal static string BuffPositionId => Loc.T( "Buff Bar" ) + "##BuffPosition";

        private static void SetBuffPosition( Vector2 pos ) {
            JobBars.SetWindowPosition( BuffPositionId, pos );
            JobBars.Configuration.BuffPosition = pos;
            JobBars.Configuration.Save();
        }
    }
}
