using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020000A9 RID: 169
[Serializable]
public struct OptionVideoContainer
{
	// Token: 0x040004B0 RID: 1200
	public Image mOrange;

	// Token: 0x040004B1 RID: 1201
	public UITween mSelected;

	// Token: 0x040004B2 RID: 1202
	public RectTransform mBoundaries;

	// Token: 0x040004B3 RID: 1203
	public Image[] mArrows;

	// Token: 0x040004B4 RID: 1204
	public eOptionVideo mDataValue;
}
