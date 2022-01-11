using System;
using System.Collections.Generic;
using BansheeGz.BGDatabase;

//=============================================================
//||                   Generated by BansheeGz Code Generator ||
//=============================================================

#pragma warning disable 414

//=============================================================
//||                   Generated by BansheeGz Code Generator ||
//=============================================================

public partial class E_weapon_info : BGEntity
{

	//=============================================================
	//||                   Generated by BansheeGz Code Generator ||
	//=============================================================

	public class Factory : BGEntity.EntityFactory
	{
		public BGEntity NewEntity(BGMetaEntity meta)
		{
			return new E_weapon_info(meta);
		}
		public BGEntity NewEntity(BGMetaEntity meta, BGId id)
		{
			return new E_weapon_info(meta, id);
		}
	}
	private static BansheeGz.BGDatabase.BGMetaRow _metaDefault;
	public static BansheeGz.BGDatabase.BGMetaRow MetaDefault
	{
		get
		{
			if(_metaDefault==null || _metaDefault.IsDeleted) _metaDefault=BGRepo.I.GetMeta<BansheeGz.BGDatabase.BGMetaRow>(new BGId(4670630178216230562UL,6096936188895372698UL));
			return _metaDefault;
		}
	}
	public static BansheeGz.BGDatabase.BGRepoEvents Events
	{
		get
		{
			return BGRepo.I.Events;
		}
	}
	private static readonly List<BGEntity> _find_Entities_Result = new List<BGEntity>();
	public static int CountEntities
	{
		get
		{
			return MetaDefault.CountEntities;
		}
	}
	public System.String f_name
	{
		get
		{
			return _f_name[Index];
		}
		set
		{
			_f_name[Index] = value;
		}
	}
	public System.Int32 f_base_damage
	{
		get
		{
			return _f_base_damage[Index];
		}
		set
		{
			_f_base_damage[Index] = value;
		}
	}
	public System.Single f_range_modifier
	{
		get
		{
			return _f_range_modifier[Index];
		}
		set
		{
			_f_range_modifier[Index] = value;
		}
	}
	public System.Boolean f_is_semi_auto
	{
		get
		{
			return _f_is_semi_auto[Index];
		}
		set
		{
			_f_is_semi_auto[Index] = value;
		}
	}
	public System.Int32 f_clip_size
	{
		get
		{
			return _f_clip_size[Index];
		}
		set
		{
			_f_clip_size[Index] = value;
		}
	}
	public System.Int32 f_pallet_per_shot
	{
		get
		{
			return _f_pallet_per_shot[Index];
		}
		set
		{
			_f_pallet_per_shot[Index] = value;
		}
	}
	public System.Single f_recoil
	{
		get
		{
			return _f_recoil[Index];
		}
		set
		{
			_f_recoil[Index] = value;
		}
	}
	public System.Single f_camera_shake
	{
		get
		{
			return _f_camera_shake[Index];
		}
		set
		{
			_f_camera_shake[Index] = value;
		}
	}
	public System.Single f_spread
	{
		get
		{
			return _f_spread[Index];
		}
		set
		{
			_f_spread[Index] = value;
		}
	}
	public System.Single f_spread_move
	{
		get
		{
			return _f_spread_move[Index];
		}
		set
		{
			_f_spread_move[Index] = value;
		}
	}
	public System.Single f_draw_time
	{
		get
		{
			return _f_draw_time[Index];
		}
		set
		{
			_f_draw_time[Index] = value;
		}
	}
	public System.Single f_shoot_interval
	{
		get
		{
			return _f_shoot_interval[Index];
		}
		set
		{
			_f_shoot_interval[Index] = value;
		}
	}
	public System.Single f_reload_time
	{
		get
		{
			return _f_reload_time[Index];
		}
		set
		{
			_f_reload_time[Index] = value;
		}
	}
	public System.Single f_reload_time_pallet_start
	{
		get
		{
			return _f_reload_time_pallet_start[Index];
		}
		set
		{
			_f_reload_time_pallet_start[Index] = value;
		}
	}
	public System.Single f_reload_time_pallet_insert
	{
		get
		{
			return _f_reload_time_pallet_insert[Index];
		}
		set
		{
			_f_reload_time_pallet_insert[Index] = value;
		}
	}
	public System.Single f_reload_time_pallet_end
	{
		get
		{
			return _f_reload_time_pallet_end[Index];
		}
		set
		{
			_f_reload_time_pallet_end[Index] = value;
		}
	}
	public WeaponCategory f_category
	{
		get
		{
			return (WeaponCategory) _f_category[Index];
		}
		set
		{
			_f_category[Index] = value;
		}
	}
	public WeaponReloadType f_reload_type
	{
		get
		{
			return (WeaponReloadType) _f_reload_type[Index];
		}
		set
		{
			_f_reload_type[Index] = value;
		}
	}
	public System.Int32 f_progression_tier
	{
		get
		{
			return _f_progression_tier[Index];
		}
		set
		{
			_f_progression_tier[Index] = value;
		}
	}
	public System.Int32 f_dm_kill_score
	{
		get
		{
			return _f_dm_kill_score[Index];
		}
		set
		{
			_f_dm_kill_score[Index] = value;
		}
	}
	public System.Boolean f_active
	{
		get
		{
			return _f_active[Index];
		}
		set
		{
			_f_active[Index] = value;
		}
	}
	private static BansheeGz.BGDatabase.BGFieldEntityName _ufle12jhs77_f_name;
	public static BansheeGz.BGDatabase.BGFieldEntityName _f_name
	{
		get
		{
			if(_ufle12jhs77_f_name==null || _ufle12jhs77_f_name.IsDeleted) _ufle12jhs77_f_name=(BansheeGz.BGDatabase.BGFieldEntityName) MetaDefault.GetField(new BGId(5074220435775288759UL,6103681286880476341UL));
			return _ufle12jhs77_f_name;
		}
	}
	private static BansheeGz.BGDatabase.BGFieldInt _ufle12jhs77_f_base_damage;
	public static BansheeGz.BGDatabase.BGFieldInt _f_base_damage
	{
		get
		{
			if(_ufle12jhs77_f_base_damage==null || _ufle12jhs77_f_base_damage.IsDeleted) _ufle12jhs77_f_base_damage=(BansheeGz.BGDatabase.BGFieldInt) MetaDefault.GetField(new BGId(4890035645047449932UL,3090094229568220042UL));
			return _ufle12jhs77_f_base_damage;
		}
	}
	private static BansheeGz.BGDatabase.BGFieldFloat _ufle12jhs77_f_range_modifier;
	public static BansheeGz.BGDatabase.BGFieldFloat _f_range_modifier
	{
		get
		{
			if(_ufle12jhs77_f_range_modifier==null || _ufle12jhs77_f_range_modifier.IsDeleted) _ufle12jhs77_f_range_modifier=(BansheeGz.BGDatabase.BGFieldFloat) MetaDefault.GetField(new BGId(4683219957302565823UL,13678527264954651022UL));
			return _ufle12jhs77_f_range_modifier;
		}
	}
	private static BansheeGz.BGDatabase.BGFieldBool _ufle12jhs77_f_is_semi_auto;
	public static BansheeGz.BGDatabase.BGFieldBool _f_is_semi_auto
	{
		get
		{
			if(_ufle12jhs77_f_is_semi_auto==null || _ufle12jhs77_f_is_semi_auto.IsDeleted) _ufle12jhs77_f_is_semi_auto=(BansheeGz.BGDatabase.BGFieldBool) MetaDefault.GetField(new BGId(4840471375314749571UL,6716578820605775798UL));
			return _ufle12jhs77_f_is_semi_auto;
		}
	}
	private static BansheeGz.BGDatabase.BGFieldInt _ufle12jhs77_f_clip_size;
	public static BansheeGz.BGDatabase.BGFieldInt _f_clip_size
	{
		get
		{
			if(_ufle12jhs77_f_clip_size==null || _ufle12jhs77_f_clip_size.IsDeleted) _ufle12jhs77_f_clip_size=(BansheeGz.BGDatabase.BGFieldInt) MetaDefault.GetField(new BGId(5527103663651466065UL,10923072305087813514UL));
			return _ufle12jhs77_f_clip_size;
		}
	}
	private static BansheeGz.BGDatabase.BGFieldInt _ufle12jhs77_f_pallet_per_shot;
	public static BansheeGz.BGDatabase.BGFieldInt _f_pallet_per_shot
	{
		get
		{
			if(_ufle12jhs77_f_pallet_per_shot==null || _ufle12jhs77_f_pallet_per_shot.IsDeleted) _ufle12jhs77_f_pallet_per_shot=(BansheeGz.BGDatabase.BGFieldInt) MetaDefault.GetField(new BGId(5368421919854404419UL,1784747819817563808UL));
			return _ufle12jhs77_f_pallet_per_shot;
		}
	}
	private static BansheeGz.BGDatabase.BGFieldFloat _ufle12jhs77_f_recoil;
	public static BansheeGz.BGDatabase.BGFieldFloat _f_recoil
	{
		get
		{
			if(_ufle12jhs77_f_recoil==null || _ufle12jhs77_f_recoil.IsDeleted) _ufle12jhs77_f_recoil=(BansheeGz.BGDatabase.BGFieldFloat) MetaDefault.GetField(new BGId(5306463085079295087UL,6167952524468047003UL));
			return _ufle12jhs77_f_recoil;
		}
	}
	private static BansheeGz.BGDatabase.BGFieldFloat _ufle12jhs77_f_camera_shake;
	public static BansheeGz.BGDatabase.BGFieldFloat _f_camera_shake
	{
		get
		{
			if(_ufle12jhs77_f_camera_shake==null || _ufle12jhs77_f_camera_shake.IsDeleted) _ufle12jhs77_f_camera_shake=(BansheeGz.BGDatabase.BGFieldFloat) MetaDefault.GetField(new BGId(5719258023115973339UL,11089364542864385422UL));
			return _ufle12jhs77_f_camera_shake;
		}
	}
	private static BansheeGz.BGDatabase.BGFieldFloat _ufle12jhs77_f_spread;
	public static BansheeGz.BGDatabase.BGFieldFloat _f_spread
	{
		get
		{
			if(_ufle12jhs77_f_spread==null || _ufle12jhs77_f_spread.IsDeleted) _ufle12jhs77_f_spread=(BansheeGz.BGDatabase.BGFieldFloat) MetaDefault.GetField(new BGId(5052825553567064992UL,7269493519236057988UL));
			return _ufle12jhs77_f_spread;
		}
	}
	private static BansheeGz.BGDatabase.BGFieldFloat _ufle12jhs77_f_spread_move;
	public static BansheeGz.BGDatabase.BGFieldFloat _f_spread_move
	{
		get
		{
			if(_ufle12jhs77_f_spread_move==null || _ufle12jhs77_f_spread_move.IsDeleted) _ufle12jhs77_f_spread_move=(BansheeGz.BGDatabase.BGFieldFloat) MetaDefault.GetField(new BGId(5728888798148707652UL,11861507793609491092UL));
			return _ufle12jhs77_f_spread_move;
		}
	}
	private static BansheeGz.BGDatabase.BGFieldFloat _ufle12jhs77_f_draw_time;
	public static BansheeGz.BGDatabase.BGFieldFloat _f_draw_time
	{
		get
		{
			if(_ufle12jhs77_f_draw_time==null || _ufle12jhs77_f_draw_time.IsDeleted) _ufle12jhs77_f_draw_time=(BansheeGz.BGDatabase.BGFieldFloat) MetaDefault.GetField(new BGId(5071738783371732345UL,8331559641230338984UL));
			return _ufle12jhs77_f_draw_time;
		}
	}
	private static BansheeGz.BGDatabase.BGFieldFloat _ufle12jhs77_f_shoot_interval;
	public static BansheeGz.BGDatabase.BGFieldFloat _f_shoot_interval
	{
		get
		{
			if(_ufle12jhs77_f_shoot_interval==null || _ufle12jhs77_f_shoot_interval.IsDeleted) _ufle12jhs77_f_shoot_interval=(BansheeGz.BGDatabase.BGFieldFloat) MetaDefault.GetField(new BGId(5349967516016039171UL,17838869606908931495UL));
			return _ufle12jhs77_f_shoot_interval;
		}
	}
	private static BansheeGz.BGDatabase.BGFieldFloat _ufle12jhs77_f_reload_time;
	public static BansheeGz.BGDatabase.BGFieldFloat _f_reload_time
	{
		get
		{
			if(_ufle12jhs77_f_reload_time==null || _ufle12jhs77_f_reload_time.IsDeleted) _ufle12jhs77_f_reload_time=(BansheeGz.BGDatabase.BGFieldFloat) MetaDefault.GetField(new BGId(4900973595100836928UL,14431906305716465030UL));
			return _ufle12jhs77_f_reload_time;
		}
	}
	private static BansheeGz.BGDatabase.BGFieldFloat _ufle12jhs77_f_reload_time_pallet_start;
	public static BansheeGz.BGDatabase.BGFieldFloat _f_reload_time_pallet_start
	{
		get
		{
			if(_ufle12jhs77_f_reload_time_pallet_start==null || _ufle12jhs77_f_reload_time_pallet_start.IsDeleted) _ufle12jhs77_f_reload_time_pallet_start=(BansheeGz.BGDatabase.BGFieldFloat) MetaDefault.GetField(new BGId(5558194620638298767UL,6203067537317166008UL));
			return _ufle12jhs77_f_reload_time_pallet_start;
		}
	}
	private static BansheeGz.BGDatabase.BGFieldFloat _ufle12jhs77_f_reload_time_pallet_insert;
	public static BansheeGz.BGDatabase.BGFieldFloat _f_reload_time_pallet_insert
	{
		get
		{
			if(_ufle12jhs77_f_reload_time_pallet_insert==null || _ufle12jhs77_f_reload_time_pallet_insert.IsDeleted) _ufle12jhs77_f_reload_time_pallet_insert=(BansheeGz.BGDatabase.BGFieldFloat) MetaDefault.GetField(new BGId(4911405929281830021UL,2494390527557722499UL));
			return _ufle12jhs77_f_reload_time_pallet_insert;
		}
	}
	private static BansheeGz.BGDatabase.BGFieldFloat _ufle12jhs77_f_reload_time_pallet_end;
	public static BansheeGz.BGDatabase.BGFieldFloat _f_reload_time_pallet_end
	{
		get
		{
			if(_ufle12jhs77_f_reload_time_pallet_end==null || _ufle12jhs77_f_reload_time_pallet_end.IsDeleted) _ufle12jhs77_f_reload_time_pallet_end=(BansheeGz.BGDatabase.BGFieldFloat) MetaDefault.GetField(new BGId(4687962139478950321UL,17673524966107817662UL));
			return _ufle12jhs77_f_reload_time_pallet_end;
		}
	}
	private static BansheeGz.BGDatabase.BGFieldEnum _ufle12jhs77_f_category;
	public static BansheeGz.BGDatabase.BGFieldEnum _f_category
	{
		get
		{
			if(_ufle12jhs77_f_category==null || _ufle12jhs77_f_category.IsDeleted) _ufle12jhs77_f_category=(BansheeGz.BGDatabase.BGFieldEnum) MetaDefault.GetField(new BGId(5530380322779058955UL,2417539453599107204UL));
			return _ufle12jhs77_f_category;
		}
	}
	private static BansheeGz.BGDatabase.BGFieldEnum _ufle12jhs77_f_reload_type;
	public static BansheeGz.BGDatabase.BGFieldEnum _f_reload_type
	{
		get
		{
			if(_ufle12jhs77_f_reload_type==null || _ufle12jhs77_f_reload_type.IsDeleted) _ufle12jhs77_f_reload_type=(BansheeGz.BGDatabase.BGFieldEnum) MetaDefault.GetField(new BGId(5292676369794396765UL,3228840254846382227UL));
			return _ufle12jhs77_f_reload_type;
		}
	}
	private static BansheeGz.BGDatabase.BGFieldInt _ufle12jhs77_f_progression_tier;
	public static BansheeGz.BGDatabase.BGFieldInt _f_progression_tier
	{
		get
		{
			if(_ufle12jhs77_f_progression_tier==null || _ufle12jhs77_f_progression_tier.IsDeleted) _ufle12jhs77_f_progression_tier=(BansheeGz.BGDatabase.BGFieldInt) MetaDefault.GetField(new BGId(5154641768473479024UL,4415341554901312417UL));
			return _ufle12jhs77_f_progression_tier;
		}
	}
	private static BansheeGz.BGDatabase.BGFieldInt _ufle12jhs77_f_dm_kill_score;
	public static BansheeGz.BGDatabase.BGFieldInt _f_dm_kill_score
	{
		get
		{
			if(_ufle12jhs77_f_dm_kill_score==null || _ufle12jhs77_f_dm_kill_score.IsDeleted) _ufle12jhs77_f_dm_kill_score=(BansheeGz.BGDatabase.BGFieldInt) MetaDefault.GetField(new BGId(4694662279016321779UL,11883779653410052013UL));
			return _ufle12jhs77_f_dm_kill_score;
		}
	}
	private static BansheeGz.BGDatabase.BGFieldBool _ufle12jhs77_f_active;
	public static BansheeGz.BGDatabase.BGFieldBool _f_active
	{
		get
		{
			if(_ufle12jhs77_f_active==null || _ufle12jhs77_f_active.IsDeleted) _ufle12jhs77_f_active=(BansheeGz.BGDatabase.BGFieldBool) MetaDefault.GetField(new BGId(5660476538095711682UL,17912296700270086335UL));
			return _ufle12jhs77_f_active;
		}
	}
	private static readonly E_weapon_info.Factory _factory0_PFS = new E_weapon_info.Factory();
	private E_weapon_info() : base(MetaDefault)
	{
	}
	private E_weapon_info(BGId id) : base(MetaDefault, id)
	{
	}
	private E_weapon_info(BGMetaEntity meta) : base(meta)
	{
	}
	private E_weapon_info(BGMetaEntity meta, BGId id) : base(meta, id)
	{
	}
	public static E_weapon_info FindEntity(Predicate<E_weapon_info> filter)
	{
		return MetaDefault.FindEntity(entity => filter==null || filter((E_weapon_info) entity)) as E_weapon_info;
	}
	public static List<E_weapon_info> FindEntities(Predicate<E_weapon_info> filter, List<E_weapon_info> result=null, Comparison<E_weapon_info> sort=null)
	{
		result = result ?? new List<E_weapon_info>();
		_find_Entities_Result.Clear();
		MetaDefault.FindEntities(filter == null ? (Predicate<BGEntity>) null: e => filter((E_weapon_info) e), _find_Entities_Result, sort == null ? (Comparison<BGEntity>) null : (e1, e2) => sort((E_weapon_info) e1, (E_weapon_info) e2));
		if (_find_Entities_Result.Count != 0)
		{
			for (var i = 0; i < _find_Entities_Result.Count; i++) result.Add((E_weapon_info) _find_Entities_Result[i]);
			_find_Entities_Result.Clear();
		}
		return result;
	}
	public static void ForEachEntity(Action<E_weapon_info> action, Predicate<E_weapon_info> filter=null, Comparison<E_weapon_info> sort=null)
	{
		MetaDefault.ForEachEntity(entity => action((E_weapon_info) entity), filter == null ? null : (Predicate<BGEntity>) (entity => filter((E_weapon_info) entity)), sort==null?(Comparison<BGEntity>) null:(e1,e2) => sort((E_weapon_info)e1,(E_weapon_info)e2));
	}
	public static E_weapon_info GetEntity(BGId entityId)
	{
		return (E_weapon_info) MetaDefault.GetEntity(entityId);
	}
	public static E_weapon_info GetEntity(int index)
	{
		return (E_weapon_info) MetaDefault[index];
	}
	public static E_weapon_info GetEntity(string entityName)
	{
		return (E_weapon_info) MetaDefault.GetEntity(entityName);
	}
	public static E_weapon_info NewEntity()
	{
		return (E_weapon_info) MetaDefault.NewEntity();
	}
}
#pragma warning restore 414
