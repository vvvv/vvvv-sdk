using System;
using System.Drawing;

namespace VVVV.Core.View
{
    public class DefaultDecoratableProvider : IDecoratable
    {
        private readonly ModelMapper FMapper;
        
        public DefaultDecoratableProvider(ModelMapper mapper)
        {
            FMapper = mapper;
            
            if (FMapper.CanMap<INamed>())
                Text = FMapper.Map<INamed>().Name;
            else
                Text = FMapper.Model.ToString();
        }
        
        public event DecorationChangedHandler DecorationChanged;
        
        protected virtual void OnDecorationChanged()
        {
            if (DecorationChanged != null) {
                DecorationChanged();
            }
        }
        
        public Pen TextColor {
            get {
                return Pens.Black;
            }
        }
        
        public Pen TextHoverColor {
            get {
                return Pens.White;
            }
        }
        
        public Brush BackColor {
            get {
                return Brushes.DarkGray;
            }
        }
        
        public Brush BackHoverColor {
            get {
                return Brushes.Gray;
            }
        }
        
        public Pen OutlineColor {
            get {
                return Pens.Red;
            }
        }
        
        public string Text {
            get;
            private set;
        }
        
        public NodeIcon Icon {
            get {
                return NodeIcon.None;
            }
        }
    }
}
