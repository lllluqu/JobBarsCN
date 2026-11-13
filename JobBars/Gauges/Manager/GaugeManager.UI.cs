using Dalamud.Bindings.ImGui;
using JobBars.Data;
using JobBars.Helper;
using System;
using System.Numerics;

namespace JobBars.Gauges.Manager {
    public partial class GaugeManager {
        public bool LOCKED = true;

        private static readonly GaugePositionType[] ValidGaugePositionType = ( GaugePositionType[] )Enum.GetValues( typeof( GaugePositionType ) );

        private readonly InfoBox<GaugeManager> PositionInfoBox = new() {
            Label = "Position",
            ContentsAction = manager => {
                ImGui.Checkbox( Loc.T( "Position locked" ) + manager.Id, ref manager.LOCKED );

                if( JobBars.Configuration.GaugePositionType != GaugePositionType.Split ) {
                    if( ImGui.Checkbox( Loc.T( "Horizontal gauges" ), ref JobBars.Configuration.GaugeHorizontal ) ) {
                        JobBars.Configuration.Save();
                    }

                    if( ImGui.Checkbox( Loc.T( "Bottom-to-top" ), ref JobBars.Configuration.GaugeBottomToTop ) ) {
                        JobBars.Configuration.Save();
                    }

                    if( ImGui.Checkbox( Loc.T( "Align right" ), ref JobBars.Configuration.GaugeAlignRight ) ) {
                        JobBars.Configuration.Save();
                    }
                }

                if( JobBars.DrawCombo( ValidGaugePositionType, JobBars.Configuration.GaugePositionType, Loc.T( "Gauge positioning" ), manager.Id, out var newPosition ) ) {
                    JobBars.Configuration.GaugePositionType = newPosition;
                    JobBars.Configuration.Save();
                }

                if( JobBars.Configuration.GaugePositionType == GaugePositionType.Global ) { // GLOBAL POSITIONING
                    var pos = JobBars.Configuration.GaugePositionGlobal;
                    if( ImGui.InputFloat2( Loc.T( "Position" ) + manager.Id, ref pos ) ) {
                        SetGaugePositionGlobal( pos );
                    }
                }
                else if( JobBars.Configuration.GaugePositionType == GaugePositionType.PerJob ) { // PER-JOB POSITIONING
                    var pos = manager.GetPerJobPosition();
                    if( ImGui.InputFloat2( Loc.T( "Position ({0})", UiHelper.JobDisplay( manager.CurrentJob ) ) + manager.Id, ref pos ) ) {
                        SetGaugePositionPerJob( manager.CurrentJob, pos );
                    }
                }

                if( ImGui.InputFloat( Loc.T( "Scale" ) + manager.Id, ref JobBars.Configuration.GaugeScale ) ) {
                    JobBars.Configuration.Save();
                }
            }
        };

        private readonly InfoBox<GaugeManager> HideWhenInfoBox = new() {
            Label = "Hide When",
            ContentsAction = manager => {
                if( ImGui.Checkbox( Loc.T( "Out of combat" ), ref JobBars.Configuration.GaugesHideOutOfCombat ) ) JobBars.Configuration.Save();
                if( ImGui.Checkbox( Loc.T( "Weapon sheathed" ), ref JobBars.Configuration.GaugesHideWeaponSheathed ) ) JobBars.Configuration.Save();
            }
        };

        protected override void DrawHeader() {
            if( ImGui.Checkbox( Loc.T( "Gauges Enabled" ) + Id, ref JobBars.Configuration.GaugesEnabled ) ) {
                JobBars.Configuration.Save();
            }
        }

        protected override void DrawSettings() {
            PositionInfoBox.Draw( this );
            HideWhenInfoBox.Draw( this );

            if( ImGui.Checkbox( Loc.T( "Pulse diamond and arrow color" ), ref JobBars.Configuration.GaugePulse ) ) JobBars.Configuration.Save();

            ImGui.SetNextItemWidth( 50f );
            if( ImGui.InputFloat( Loc.T( "Slidecast seconds (0 = off)" ), ref JobBars.Configuration.GaugeSlidecastTime ) ) JobBars.Configuration.Save();
        }

        public void DrawPositionBox() {
            if( LOCKED ) return;
            if( JobBars.Configuration.GaugePositionType == GaugePositionType.Split ) {
                foreach( var config in CurrentConfigs ) config.DrawPositionBox();
            }
            else if( JobBars.Configuration.GaugePositionType == GaugePositionType.PerJob ) {
                var currentPos = GetPerJobPosition();
                if( JobBars.DrawPositionView( GaugePositionId( CurrentJob ), currentPos, out var pos ) ) {
                    SetGaugePositionPerJob( CurrentJob, pos );
                }
            }
            else { // GLOBAL
                var currentPos = JobBars.Configuration.GaugePositionGlobal;
                if( JobBars.DrawPositionView( GaugePositionId( null ), currentPos, out var pos ) ) {
                    SetGaugePositionGlobal( pos );
                }
            }
        }

        internal static string GaugePositionId( JobIds? job ) =>
            job is null
                ? Loc.T( "Gauge Bar" ) + "##GaugePosition"
                : $"{Loc.T( "Gauge Bar" )} ({UiHelper.JobDisplay( job.Value )})##GaugePosition";

        private static void SetGaugePositionGlobal( Vector2 pos ) {
            JobBars.SetWindowPosition( GaugePositionId( null ), pos );
            JobBars.Configuration.GaugePositionGlobal = pos;
            JobBars.Configuration.Save();
        }

        private static void SetGaugePositionPerJob( JobIds job, Vector2 pos ) {
            JobBars.SetWindowPosition( GaugePositionId( job ), pos );
            JobBars.Configuration.GaugePerJobPosition.Set( $"{job}", pos );
            JobBars.Configuration.Save();
        }

        // ==========================================

        protected override void DrawItem( GaugeConfig item ) {
            ImGui.Indent( 5 );
            item.Draw( Id, out var newVisual, out var reset );
            ImGui.Unindent();

            if( SelectedJob != CurrentJob ) return;
            if( reset ) Reset();
        }

        protected override string ItemToString( GaugeConfig item ) => UiHelper.DisplayName( item.Name );

        protected override bool IsEnabled( GaugeConfig item ) => item.Enabled;
    }
}
