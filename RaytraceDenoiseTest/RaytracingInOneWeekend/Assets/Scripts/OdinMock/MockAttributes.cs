
using System;

namespace OdinMock
{
	public class ShowIfAttribute : Attribute
	{
		public ShowIfAttribute(string _, object __) { }
		public ShowIfAttribute(string _) { }
	}

	public class HideIfAttribute : Attribute
	{
		public HideIfAttribute(string _) { }

		public HideIfAttribute(string _, object __) { }
	}

	public class MinMaxSliderAttribute : Attribute
	{
		public MinMaxSliderAttribute(float _, float __, bool ___) { }
		public MinMaxSliderAttribute(float _, float __) { }

	}

	public class LabelWidthAttribute : Attribute
	{
		public LabelWidthAttribute(float _) { }
	}


	public class InlineButtonAttribute : Attribute
	{
		public InlineButtonAttribute(string _, string __) { }
	}

	public class InlineEditorAttribute : Attribute
	{
		public bool DrawHeader;
	}

	public class ShowInInspectorAttribute : Attribute
	{
	}
	public class AssetListAttribute : Attribute
	{
	}

	public class ReadOnlyAttribute : Attribute
	{
	}

	public class AssetsOnlyAttribute : Attribute
	{
	}

	public class LabelTextAttribute : Attribute
	{
		public LabelTextAttribute(string _) { }
	}

	public class BoxGroupAttribute : Attribute
	{
		public BoxGroupAttribute(string _) { }
	}

	public class ValueDropdownAttribute : Attribute
	{
		public ValueDropdownAttribute(string _) { }
	}

	public class DisableInPlayModeAttribute : Attribute
	{
	}

	public class HideLabelAttribute : Attribute
	{
	}

	public class DisableInEditorModeAttribute : Attribute
	{
	}

	public class ButtonAttribute : Attribute
	{
	}
    public class FoldoutGroupAttribute : Attribute
    {
        public FoldoutGroupAttribute(string _) { }

    }

	public class DisableIfAttribute : Attribute
	{
		public DisableIfAttribute(string _) { }
	}

	public class EnableIfAttribute : Attribute
	{
		public EnableIfAttribute(string _) { }
	}

	public class ButtonGroupAttribute : Attribute
	{
		public ButtonGroupAttribute(string _) { }
		public ButtonGroupAttribute(string _, int __) { }

	}

	public class TitleAttribute : Attribute
	{
		public TitleAttribute(string _) { }
	}
}
