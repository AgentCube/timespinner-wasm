using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.Xna.Framework;
using Timespinner.GameAbstractions;

namespace Timespinner.Core.Specifications;

[Serializable]
public class BackgroundSpecification
{
	public const string XmlNameAttribute = "Name";

	public const string XmlTextureTypeAttribute = "TextureType";

	public const string XmlIsForegroundAttribute = "IsForeground";

	public const string XmlIsHorizontallyFlippedAttribute = "IsHorizontallyFlipped";

	public const string XmlIsVerticallyFlippedAttribute = "IsVerticallyFlipped";

	public const string XmlDoesAlternateXFlip = "DoesAlternateXFlip";

	public const string XmlDoesAlternateYFlip = "DoesAlternateYFlip";

	public const string XmlDoesTileWestAttribute = "DoesTileWest";

	public const string XmlDoesTileNorthAttribute = "DoesTileNorth";

	public const string XmlDoesTileEastAttribute = "DoesTileEast";

	public const string XmlDoesTileSouthAttribute = "DoesTileSouth";

	public const string XmlFrameIndexAttribute = "FrameIndex";

	public const string XmlZoomAttribute = "Zoom";

	public const string XmlTileIntervalAttribute = "TileInterval";

	public const string XmlPanRatioAttribute = "PanRatio";

	public const string XmlStartPointAttribute = "StartPoint";

	public const string XmlEndPointAttribute = "EndPoint";

	public const string XmlScrollSpeedAttribute = "ScrollSpeed";

	public const string XmlStretchRectangleAttribute = "StretchRectangle";

	public const string XmlDrawColorAttribute = "DrawColor";

	public EBackgroundTextureType TextureType { get; set; }

	public bool IsForeground { get; set; }

	public bool IsHorizontallyFlipped { get; set; }

	public bool IsVerticallyFlipped { get; set; }

	public bool DoesAlternateXFlip { get; set; }

	public bool DoesAlternateYFlip { get; set; }

	public bool DoesTileWest { get; set; }

	public bool DoesTileNorth { get; set; }

	public bool DoesTileEast { get; set; }

	public bool DoesTileSouth { get; set; }

	public int ID { get; set; }

	public int FrameIndex { get; set; }

	public float Zoom { get; set; }

	public string Name { get; set; }

	public Point TileInterval { get; set; }

	public Point StartPoint { get; set; }

	public Point EndPoint { get; set; }

	public Vector2 PanRatio { get; set; }

	public Vector2 ScrollSpeed { get; set; }

	public Rectangle StretchRectangle { get; set; }

	public Color DrawColor { get; set; }

	public List<SwitchSpecification> Switches { get; set; }

	public BackgroundSpecification()
	{
		PanRatio = new Vector2(1f, 1f);
		DrawColor = Color.White;
		Switches = new List<SwitchSpecification>();
	}

	public static BackgroundSpecification FromXml(XmlReader reader)
	{
		BackgroundSpecification backgroundSpecification = new BackgroundSpecification();
		while (reader.MoveToNextAttribute())
		{
			switch (reader.Name)
			{
			case "Name":
				backgroundSpecification.Name = reader.Value;
				break;
			case "TextureType":
				backgroundSpecification.TextureType = EnumExtensions.EnumParse<EBackgroundTextureType>(reader.Value);
				break;
			case "IsForeground":
				backgroundSpecification.IsForeground = bool.Parse(reader.Value);
				break;
			case "IsHorizontallyFlipped":
				backgroundSpecification.IsHorizontallyFlipped = bool.Parse(reader.Value);
				break;
			case "IsVerticallyFlipped":
				backgroundSpecification.IsVerticallyFlipped = bool.Parse(reader.Value);
				break;
			case "DoesAlternateXFlip":
				backgroundSpecification.DoesAlternateXFlip = bool.Parse(reader.Value);
				break;
			case "DoesAlternateYFlip":
				backgroundSpecification.DoesAlternateYFlip = bool.Parse(reader.Value);
				break;
			case "DoesTileWest":
				backgroundSpecification.DoesTileWest = bool.Parse(reader.Value);
				break;
			case "DoesTileNorth":
				backgroundSpecification.DoesTileNorth = bool.Parse(reader.Value);
				break;
			case "DoesTileEast":
				backgroundSpecification.DoesTileEast = bool.Parse(reader.Value);
				break;
			case "DoesTileSouth":
				backgroundSpecification.DoesTileSouth = bool.Parse(reader.Value);
				break;
			case "FrameIndex":
				backgroundSpecification.FrameIndex = reader.Value.ParseInt32();
				break;
			case "Zoom":
				backgroundSpecification.Zoom = reader.Value.ParseFloat();
				break;
			case "TileInterval":
				backgroundSpecification.TileInterval = MathEx.ParsePoint(reader.Value);
				break;
			case "PanRatio":
				backgroundSpecification.PanRatio = MathEx.ParseVector2(reader.Value);
				break;
			case "StartPoint":
				backgroundSpecification.StartPoint = MathEx.ParsePoint(reader.Value);
				break;
			case "EndPoint":
				backgroundSpecification.EndPoint = MathEx.ParsePoint(reader.Value);
				break;
			case "ScrollSpeed":
				backgroundSpecification.ScrollSpeed = MathEx.ParseVector2(reader.Value);
				break;
			case "StretchRectangle":
				backgroundSpecification.StretchRectangle = MathEx.ParseRectangle(reader.Value);
				break;
			case "DrawColor":
				backgroundSpecification.DrawColor = MathEx.ParseColor(reader.Value);
				break;
			}
		}
		if (backgroundSpecification != null && string.IsNullOrEmpty(backgroundSpecification.Name))
		{
			backgroundSpecification.Name = backgroundSpecification.TextureType.ToString();
		}
		return backgroundSpecification;
	}

	public SpriteSheet GetSpriteFromType(GCM gcm)
	{
		return GetSpriteFromType(TextureType, gcm);
	}

	public static SpriteSheet GetSpriteFromType(EBackgroundTextureType type, GCM gcm)
	{
		SpriteSheet result = null;
		switch (type)
		{
		case EBackgroundTextureType.L1_Backdrop1:
			result = gcm.BgL1Backdrop1;
			break;
		case EBackgroundTextureType.L1_Backdrop2:
			result = gcm.BgL1Backdrop2;
			break;
		case EBackgroundTextureType.L1_Backdrop3:
			result = gcm.BgL1Backdrop3;
			break;
		case EBackgroundTextureType.L1_Backdrop4:
			result = gcm.BgL1Backdrop4;
			break;
		case EBackgroundTextureType.L1_NearBackdrop:
			result = gcm.BgL1NearBackdrop;
			break;
		case EBackgroundTextureType.L2_Fountain:
			result = gcm.BgL2Fountain;
			break;
		case EBackgroundTextureType.L6_Backdrop1:
			result = gcm.BgL6Backdrop1;
			break;
		case EBackgroundTextureType.L7_Backdrop1:
			result = gcm.BgL7Backdrop1;
			break;
		case EBackgroundTextureType.L7_Backdrop2:
			result = gcm.BgL7Backdrop2;
			break;
		case EBackgroundTextureType.L7_Backdrop3:
			result = gcm.BgL7Backdrop3;
			break;
		case EBackgroundTextureType.L7_Backdrop4:
			result = gcm.BgL7Backdrop4;
			break;
		case EBackgroundTextureType.L7_NearBackdrop:
			result = gcm.BgL7NearBackdrop;
			break;
		case EBackgroundTextureType.Temple_Backdrop1:
			result = gcm.BgTempleBackdrop1;
			break;
		case EBackgroundTextureType.Time_Backdrop1:
			result = gcm.BgTimeBackdrop1;
			break;
		case EBackgroundTextureType.Time_Backdrop2:
			result = gcm.BgTimeBackdrop2;
			break;
		case EBackgroundTextureType.Time_Backdrop3:
			result = gcm.BgTimeBackdrop3;
			break;
		case EBackgroundTextureType.Tower_Backdrops1:
			result = gcm.BgTowerBackdrops1;
			break;
		case EBackgroundTextureType.Tower_Backdrops2:
			result = gcm.BgTowerBackdrops2;
			break;
		case EBackgroundTextureType.Tower_Backdrops3:
			result = gcm.BgTowerBackdrops3;
			break;
		case EBackgroundTextureType.Tower_Backdrops4:
			result = gcm.BgTowerBackdrops4;
			break;
		case EBackgroundTextureType.Fog:
			result = gcm.BgFog;
			break;
		case EBackgroundTextureType.HalfFog:
			result = gcm.BgHalfFog;
			break;
		case EBackgroundTextureType.Clouds:
			result = gcm.BgClouds;
			break;
		case EBackgroundTextureType.Forest_Trees_Farthest:
			result = gcm.BgForestTreesFarthest;
			break;
		case EBackgroundTextureType.Forest_Trees_Far:
			result = gcm.BgForestTreesFar;
			break;
		case EBackgroundTextureType.Forest_Trees_Mid:
			result = gcm.BgForestTreesMid;
			break;
		case EBackgroundTextureType.Forest_Trees_Near:
			result = gcm.BgForestTreesNear;
			break;
		case EBackgroundTextureType.Forest_Rocks_Far:
			result = gcm.BgForestRocksFar;
			break;
		case EBackgroundTextureType.Forest_Rocks_Mid:
			result = gcm.BgForestRocksMid;
			break;
		case EBackgroundTextureType.Forest_Rocks_Near:
			result = gcm.BgForestRocksNear;
			break;
		case EBackgroundTextureType.Forest_Temple1:
			result = gcm.BgForestTemple1;
			break;
		case EBackgroundTextureType.Forest_Temple2:
			result = gcm.BgForestTemple2;
			break;
		case EBackgroundTextureType.Forest_Waterfall1:
			result = gcm.BgForestWaterfall1;
			break;
		case EBackgroundTextureType.Forest_Waterfall2:
			result = gcm.BgForestWaterfall2;
			break;
		case EBackgroundTextureType.Forest_Waterfall3:
			result = gcm.BgForestWaterfall3;
			break;
		case EBackgroundTextureType.Forest_Waterfall4:
			result = gcm.BgForestWaterfall4;
			break;
		case EBackgroundTextureType.Cave_Backdrop_Waterfall1:
			result = gcm.BgCaveBackdropWaterfall1;
			break;
		case EBackgroundTextureType.Cave_Backdrops1:
			result = gcm.BgCaveBackdrops1;
			break;
		case EBackgroundTextureType.Cave_Backdrops2:
			result = gcm.BgCaveBackdrops2;
			break;
		case EBackgroundTextureType.City_Backdrops1:
			result = gcm.BgCityBackdrops1;
			break;
		case EBackgroundTextureType.City_Backdrops2:
			result = gcm.BgCityBackdrops2;
			break;
		case EBackgroundTextureType.City_Backdrops3:
			result = gcm.BgCityBackdrops3;
			break;
		case EBackgroundTextureType.City_Backdrops4:
			result = gcm.BgCityBackdrops4;
			break;
		case EBackgroundTextureType.City_Backdrops5:
			result = gcm.BgCityBackdrops5;
			break;
		case EBackgroundTextureType.Cursed_Cave_Backdrop_Waterfall1:
			result = gcm.BgCursedCaveBackdropWaterfall1;
			break;
		case EBackgroundTextureType.Cursed_Cave_Backdrops1:
			result = gcm.BgCursedCaveBackdrops1;
			break;
		case EBackgroundTextureType.Cursed_Cave_Backdrops2:
			result = gcm.BgCursedCaveBackdrops2;
			break;
		case EBackgroundTextureType.Cursed_Cave_Backdrops3:
			result = gcm.BgCursedCaveBackdrops3;
			break;
		case EBackgroundTextureType.Curtain_Backdrops1:
			result = gcm.BgCurtainsBackdrops1;
			break;
		case EBackgroundTextureType.Curtain_Backdrops2:
			result = gcm.BgCurtainsBackdrops2;
			break;
		case EBackgroundTextureType.Curtain_Backdrops3:
			result = gcm.BgCurtainsBackdrops3;
			break;
		case EBackgroundTextureType.Keep_Backdrops1:
			result = gcm.BgKeepBackdrops1;
			break;
		case EBackgroundTextureType.Keep_Backdrops2:
			result = gcm.BgKeepBackdrops2;
			break;
		case EBackgroundTextureType.Hangar_Backdrops1:
			result = gcm.BgHangarBackdrops1;
			break;
		case EBackgroundTextureType.Hangar_Backdrops2:
			result = gcm.BgHangarBackdrops2;
			break;
		case EBackgroundTextureType.Hangar_Backdrops3:
			result = gcm.BgHangarBackdrops3;
			break;
		case EBackgroundTextureType.Lab_Backdrop1:
			result = gcm.BgLabBackdrop1;
			break;
		case EBackgroundTextureType.Emperor_Tower_Backdrops1:
			result = gcm.BgEmperorTowerBackdrops1;
			break;
		case EBackgroundTextureType.Emperor_Tower_Backdrops2:
			result = gcm.BgEmperorTowerBackdrops2;
			break;
		case EBackgroundTextureType.Emperor_Tower_Backdrops3:
			result = gcm.BgEmperorTowerBackdrops3;
			break;
		case EBackgroundTextureType.Emperor_Tower_Backdrops5:
			result = gcm.BgEmperorTowerBackdrops5;
			break;
		case EBackgroundTextureType.EndingBackdrops1:
			result = gcm.BgEndingBackdrops1;
			break;
		case EBackgroundTextureType.SpaceTiled:
			result = gcm.BgSpaceTiled;
			break;
		case EBackgroundTextureType.GyreBackdrops1:
			result = gcm.BgGyreBackdrops1;
			break;
		case EBackgroundTextureType.GyreBackdrops2:
			result = gcm.BgGyreBackdrops2;
			break;
		case EBackgroundTextureType.GyreBackdrops3:
			result = gcm.BgGyreBackdrops3;
			break;
		case EBackgroundTextureType.GyreBackdrops4:
			result = gcm.BgGyreBackdrops4;
			break;
		case EBackgroundTextureType.VileteArchways:
			result = gcm.BgVileteArchways;
			break;
		case EBackgroundTextureType.VileteBackdrop:
			result = gcm.BgVileteBackdrop;
			break;
		case EBackgroundTextureType.VileteBackdrop2:
			result = gcm.BgVileteBackdrop2;
			break;
		case EBackgroundTextureType.WinderiaBackdrop1:
			result = gcm.BgWinderiaBackdrop1;
			break;
		case EBackgroundTextureType.WinderiaBackdrop2:
			result = gcm.BgWinderiaBackdrop2;
			break;
		case EBackgroundTextureType.WinderiaBackdrop3:
			result = gcm.BgWinderiaBackdrop3;
			break;
		case EBackgroundTextureType.WinderiaBackdrop4:
			result = gcm.BgWinderiaBackdrop4;
			break;
		case EBackgroundTextureType.WinderiaBackdrop5:
			result = gcm.BgWinderiaBackdrop5;
			break;
		case EBackgroundTextureType.WinderiaBackdrop6:
			result = gcm.BgWinderiaBackdrop6;
			break;
		}
		return result;
	}

	public static EBackgroundTextureType GetTypeFromSpriteName(string name)
	{
		EBackgroundTextureType result = EBackgroundTextureType.None;
		switch (name)
		{
		case "Backgrounds/L1_Backdrop1":
			result = EBackgroundTextureType.L1_Backdrop1;
			break;
		case "Backgrounds/L1_Backdrop2":
			result = EBackgroundTextureType.L1_Backdrop2;
			break;
		case "Backgrounds/L1_NearBackdrop":
			result = EBackgroundTextureType.L1_NearBackdrop;
			break;
		case "Backgrounds/L6_Backdrop1":
			result = EBackgroundTextureType.L6_Backdrop1;
			break;
		case "Backgrounds/Time_Backdrop1":
			result = EBackgroundTextureType.Time_Backdrop1;
			break;
		case "Backgrounds/Time_Backdrop2":
			result = EBackgroundTextureType.Time_Backdrop2;
			break;
		case "Backgrounds/SpaceTiled":
			result = EBackgroundTextureType.SpaceTiled;
			break;
		}
		return result;
	}

	public BackgroundSpecification Duplicate()
	{
		BackgroundSpecification backgroundSpecification = new BackgroundSpecification();
		backgroundSpecification.TextureType = TextureType;
		backgroundSpecification.IsForeground = IsForeground;
		backgroundSpecification.IsHorizontallyFlipped = IsHorizontallyFlipped;
		backgroundSpecification.IsVerticallyFlipped = IsVerticallyFlipped;
		backgroundSpecification.DoesAlternateXFlip = DoesAlternateXFlip;
		backgroundSpecification.DoesAlternateYFlip = DoesAlternateYFlip;
		backgroundSpecification.DoesTileWest = DoesTileWest;
		backgroundSpecification.DoesTileNorth = DoesTileNorth;
		backgroundSpecification.DoesTileEast = DoesTileEast;
		backgroundSpecification.DoesTileSouth = DoesTileSouth;
		backgroundSpecification.ID = ID;
		backgroundSpecification.FrameIndex = FrameIndex;
		backgroundSpecification.Zoom = Zoom;
		backgroundSpecification.Name = Name;
		backgroundSpecification.TileInterval = TileInterval;
		backgroundSpecification.PanRatio = PanRatio;
		backgroundSpecification.StartPoint = StartPoint;
		backgroundSpecification.EndPoint = EndPoint;
		backgroundSpecification.ScrollSpeed = ScrollSpeed;
		backgroundSpecification.StretchRectangle = StretchRectangle;
		backgroundSpecification.DrawColor = DrawColor;
		BackgroundSpecification backgroundSpecification2 = backgroundSpecification;
		foreach (SwitchSpecification @switch in Switches)
		{
			backgroundSpecification2.Switches.Add(@switch.Duplicate());
		}
		return backgroundSpecification2;
	}
}
