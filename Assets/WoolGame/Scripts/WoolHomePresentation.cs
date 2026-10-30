using UnityEngine;
using UnityEngine.UI;

namespace WoolGame
{
    // Presentation only: WoolHud retains all wallet, navigation and level ownership.
    public sealed class WoolHomePresentation : MonoBehaviour
    {
        public RectTransform background, topBar, hero, actions, levelArea, navigation;
        public RectTransform dragon, mother, leftWing, rightWing, wheel, levelButton, levelName;
        public RectTransform[] actionIcons, actionCards, clouds;
        RectTransform root;
        Vector2 lastSize;
        Vector3 heroScale;
        float clock;
        float dragonBaseY=170;
        public float AnimationTime => clock;

        void OnEnable() { root=(RectTransform)transform; lastSize=Vector2.zero; Fit(); }
        public void Fit()
        {
            if(!root)root=(RectTransform)transform;
            var parent=root.parent as RectTransform;
            if(!parent||parent.rect.width<1||parent.rect.height<1)return;
            Vector2 size=parent.rect.size;
            // On tablets keep the complete composition; extra width belongs to the village.
            float scale=Mathf.Min(size.x/941f,size.y/1500f);
            float height=size.y/Mathf.Max(.001f,scale);
            root.sizeDelta=new Vector2(941,height);root.localScale=Vector3.one*scale;
            root.anchoredPosition=Vector2.zero;
            PlaceGroup(topBar,new Vector2(.5f,1),new Vector2(0,-90));
            PlaceGroup(actions,new Vector2(.5f,0),new Vector2(0,558));
            PlaceGroup(levelArea,new Vector2(.5f,0),new Vector2(0,330));
            PlaceGroup(navigation,new Vector2(.5f,0),new Vector2(0,99));
            float space=height-160-660;
            heroScale=Vector3.one*Mathf.Clamp(space/900f,.72f,1.2f);
            // Keep the family's feet beside the action cards even on very tall foldable displays.
            if(hero){PlaceGroup(hero,new Vector2(.5f,0),new Vector2(0,656+460*heroScale.x));hero.localScale=heroScale;}
            // Reserve a clear strip below the header, including hover and rotated wing tips.
            dragonBaseY=Mathf.Min(170,(space*.5f-15)/heroScale.x-264);
            if(dragon)dragon.anchoredPosition=new Vector2(365,dragonBaseY);
            if(mother)mother.localScale=Vector3.one;
            if(background)
            {
                // Background extends beneath notches and home indicators, controls do not.
                var picture=background.GetComponent<Image>();
                float aspect=picture&&picture.sprite?picture.sprite.rect.width/picture.sprite.rect.height:941f/1672f;
                WoolCanvasArt.Cover(background,aspect,1.025f);
            }
            lastSize=size;
        }
        static void PlaceGroup(RectTransform rt,Vector2 anchor,Vector2 position)
        {if(!rt)return;rt.anchorMin=rt.anchorMax=anchor;rt.anchoredPosition=position;}
        void LateUpdate()
        {
            var parent=transform.parent as RectTransform;
            if(parent&&parent.rect.size!=lastSize)Fit();
            clock+=Mathf.Min(Time.unscaledDeltaTime,.1f);
            float t=clock;
            if(background)background.localScale=Vector3.one*(1+.004f*Mathf.Sin(t*.4f));
            if(dragon)
            {
                dragon.anchoredPosition=new Vector2(365,dragonBaseY+6*Mathf.Sin(t*1.6f));
                dragon.localRotation=Quaternion.Euler(0,0,Mathf.Sin(t*1.3f)*1.2f);
            }
            // The duck and wings are one authored fixed-pose sprite.
            if(wheel)wheel.localRotation=Quaternion.Euler(0,0,-t*22);
            if(actionIcons!=null)for(int i=0;i<actionIcons.Length;i++)
            {
                var icon=actionIcons[i];if(!icon||icon==wheel)continue;
                icon.localRotation=Quaternion.Euler(0,0,Mathf.Sin(t*1.7f+i*1.4f)*2.5f);
                icon.localScale=Vector3.one*(1+.018f*Mathf.Sin(t*2+i));
            }
            if(levelButton)levelButton.localScale=Vector3.one;
            // Keep glyphs at a stable screen size while the button artwork pulses behind them.
            if(levelName&&levelButton)levelName.localScale=Vector3.one/levelButton.localScale.x;
            if(clouds!=null)for(int i=0;i<clouds.Length;i++)if(clouds[i])
            {
                float x=Mathf.Repeat(i*430+t*(7+i*2)+250,1550)-775;
                clouds[i].anchoredPosition=new Vector2(x,-150-i*84);
            }
        }
    }
}
