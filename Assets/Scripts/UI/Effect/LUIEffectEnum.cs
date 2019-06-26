
namespace UnityEngine.UI
{
    /// <summary>
    /// Blur effect mode.
    /// </summary>
    public enum BlurMode
    {
        None = 0,
        FastBlur = 1,
        MediumBlur = 2,
        DetailBlur = 3,
    }
    /// <summary>
    /// Desampling rate.
    /// </summary>
    public enum DesamplingRate
    {
        None = 0,
        x1 = 1,
        x2 = 2,
        x4 = 4,
        x8 = 8,
    }
    /// <summary>
	/// Area for effect.
	/// </summary>
	public enum EffectArea
	{
		RectTransform,
		Fit,
		Character,
	}
    /// <summary>
	/// Color effect mode.
	/// </summary>
	public enum ColorMode
	{
		Multiply = 0,
		Fill = 1,
		Add = 2,
		Subtract = 3,
	}
}