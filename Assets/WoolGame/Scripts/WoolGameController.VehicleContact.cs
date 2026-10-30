using UnityEngine;
namespace WoolGame
{
    public sealed partial class WoolGameController
    {
        sealed class ContactMove
        {
            public Vector3 start,direction;public Quaternion rotation;
            public float offset,hold,returnFrom,returnTime;
            public bool returning,wasCarried;
        }
        readonly ContactMove[] contactMoves=new ContactMove[WoolEndlessGenerator.MaxVehicles];
        float VehicleBoardScale=>definition.tutorial?1.15f:definition.boardScale;
        bool continuingContact;
        void ShowVehicleTap(int index){if(!continuingContact)hud.PlayVehicleTap(rig.vehicleCamera.WorldToScreenPoint(tileViews[index].position));}
        public int VehicleContacts{get;private set;}
        public bool VehicleIsProbing(int index)=>index>=0&&index<contactMoves.Length&&contactMoves[index]!=null;
        public bool HasFreeVehicleSlot{get{for(int i=0;i<active.Length;i++)if(SlotOpen(i)&&active[i]==null)return true;return false;}}
        static Vector3 ExitDirection(WoolDirection direction)=>direction==WoolDirection.Up?Vector3.up:direction==WoolDirection.Down?Vector3.down:direction==WoolDirection.Left?Vector3.left:Vector3.right;
        void BeginContactMove(int index)
        {
            var tile=level.Tiles[index];var actor=tileViews[index];
            var move=new ContactMove{start=actor.localPosition,rotation=actor.localRotation,wasCarried=tile.carried,
                direction=Quaternion.Euler(0,0,definition.boardRotation)*ExitDirection(tile.direction)};
            contactMoves[index]=move;tile.probing=true;tile.carried=false;
            ShowVehicleTap(index);
            actor.localRotation=Quaternion.Euler(0,0,-(int)tile.direction*90+definition.boardRotation)*Quaternion.Euler(-90,0,0);
            if(audioEnabled&&rig.departureSound)sound.PlayOneShot(rig.departureSound,.35f);
        }
        void UpdateVehicleOffset(int index)
        {
            var tile=level.Tiles[index];
            var original=rig.ParkingPresentationOffset+WoolSceneRig.ParkingOffset+new Vector3(0,-3.2f,0)+
                Quaternion.Euler(0,0,definition.boardRotation)*(new Vector3(tile.x,tile.y+3.2f,0)*VehicleBoardScale)+Vector3.back*.3f;
            var delta=Quaternion.Euler(0,0,-definition.boardRotation)*(tileViews[index].localPosition-original)/Mathf.Max(.01f,VehicleBoardScale);
            tile.motionX=delta.x;tile.motionY=delta.y;
        }
        void TickContactMoves(float dt)
        {
            dt*=VehicleSpeedMultiplier;
            for(int i=0;i<level.Tiles.Count;i++)
            {
                var move=contactMoves[i];if(move==null)continue;
                var tile=level.Tiles[i];var actor=tileViews[i];
                if(!move.returning)
                {
                    if(!level.TryGetBlocker(i,out _,out float gap))
                    {
                        contactMoves[i]=null;tile.probing=false;
                        bool moved;continuingContact=true;
                        try{moved=!removeMode&&SelectTile(i);}finally{continuingContact=false;}
                        if(moved)continue;
                        contactMoves[i]=move;tile.probing=true;move.returning=true;move.returnFrom=move.offset;
                    }
                    else
                    {
                        float remaining=Mathf.Max(0,gap*VehicleBoardScale-.018f),step=Mathf.Min(dt*14,remaining);
                        move.offset+=step;actor.localPosition=move.start+move.direction*move.offset;UpdateVehicleOffset(i);
                        if(remaining<=dt*14+.0001f)
                        {
                            move.returning=true;move.returnFrom=move.offset;move.hold=.07f;VehicleContacts++;
                            float half=tile.length*VehicleBoardScale*.5f;
                            var contact=rig.world.TransformPoint(actor.localPosition+move.direction*half);
                            hud.PlayVehicleContact(rig.vehicleCamera.WorldToScreenPoint(contact));gameAudio.Play(WoolGameAudio.Cue.Blocked);
                        }
                    }
                }
                else
                {
                    if(move.hold>0){move.hold-=dt;continue;}
                    float duration=Mathf.Clamp(move.returnFrom/14,.10f,.38f);move.returnTime+=dt;
                    float p=Mathf.Clamp01(move.returnTime/duration);move.offset=Mathf.Lerp(move.returnFrom,0,Mathf.SmoothStep(0,1,p));
                    actor.localPosition=move.start+move.direction*move.offset;UpdateVehicleOffset(i);
                    if(p>=1){actor.localPosition=move.start;actor.localRotation=move.rotation;tile.motionX=tile.motionY=0;tile.probing=false;tile.carried=move.wasCarried;contactMoves[i]=null;}
                }
            }
        }
        void ResetContactMoves()
        {
            for(int i=0;i<contactMoves.Length;i++)contactMoves[i]=null;VehicleContacts=0;
        }
    }
}
