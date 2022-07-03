using System;
using System.Collections.Generic;
using BansheeGz.BGDatabase;
using Alias_rifegrt_weapon_view_info = E_weapon_view_info;
using Alias_rifegrt_weapon_monster_info = E_weapon_monster_info;

//=============================================================
//||                   Generated by BansheeGz Code Generator ||
//=============================================================

#pragma warning disable 414

public partial class E_weapon_info : BGEntity
{

	public class Factory : BGEntity.EntityFactory
	{
		public BGEntity NewEntity(BGMetaEntity meta) => new E_weapon_info(meta);
		public BGEntity NewEntity(BGMetaEntity meta, BGId id) => new E_weapon_info(meta, id);
	}
	private static BansheeGz.BGDatabase.BGMetaRow _metaDefault;
	public static BansheeGz.BGDatabase.BGMetaRow MetaDefault => _metaDefault ?? (_metaDefault = BGCodeGenUtils.GetMeta<BansheeGz.BGDatabase.BGMetaRow>(new BGId(4670630178216230562UL,6096936188895372698UL), () => _metaDefault = null));
	public static BansheeGz.BGDatabase.BGRepoEvents Events => BGRepo.I.Events;
	public static int CountEntities => MetaDefault.CountEntities;
	public System.String f_name
	{
		get => _f_name[Index];
		set => _f_name[Index] = value;
	}
	public System.String f_display_name
	{
		get => _f_display_name[Index];
		set => _f_display_name[Index] = value;
	}
	public System.Int32 f_base_damage
	{
		get => _f_base_damage[Index];
		set => _f_base_damage[Index] = value;
	}
	public System.Single f_range_modifier
	{
		get => _f_range_modifier[Index];
		set => _f_range_modifier[Index] = value;
	}
	public System.Boolean f_is_semi_auto
	{
		get => _f_is_semi_auto[Index];
		set => _f_is_semi_auto[Index] = value;
	}
	public System.Int32 f_clip_size
	{
		get => _f_clip_size[Index];
		set => _f_clip_size[Index] = value;
	}
	public System.Int32 f_pallet_per_shot
	{
		get => _f_pallet_per_shot[Index];
		set => _f_pallet_per_shot[Index] = value;
	}
	/**<summary><![CDATA[
	Recoil push the viewport upward WITHOUT recovering , suitable for automatic weapons
	]]></summary>*/
	public System.Single f_recoil
	{
		get => _f_recoil[Index];
		set => _f_recoil[Index] = value;
	}
	/**<summary><![CDATA[
	Shake push viewport upwards , but WILL recover , suitable for non-automatic weapons
	]]></summary>*/
	public System.Single f_camera_shake
	{
		get => _f_camera_shake[Index];
		set => _f_camera_shake[Index] = value;
	}
	public System.Single f_spread
	{
		get => _f_spread[Index];
		set => _f_spread[Index] = value;
	}
	/**<summary><![CDATA[
	Additional spread when moving at full speed
	]]></summary>*/
	public System.Single f_spread_move
	{
		get => _f_spread_move[Index];
		set => _f_spread_move[Index] = value;
	}
	public System.Single f_draw_time
	{
		get => _f_draw_time[Index];
		set => _f_draw_time[Index] = value;
	}
	public System.Single f_shoot_interval
	{
		get => _f_shoot_interval[Index];
		set => _f_shoot_interval[Index] = value;
	}
	public System.Single f_reload_time
	{
		get => _f_reload_time[Index];
		set => _f_reload_time[Index] = value;
	}
	public System.Single f_reload_time_pallet_start
	{
		get => _f_reload_time_pallet_start[Index];
		set => _f_reload_time_pallet_start[Index] = value;
	}
	public System.Single f_reload_time_pallet_insert
	{
		get => _f_reload_time_pallet_insert[Index];
		set => _f_reload_time_pallet_insert[Index] = value;
	}
	public System.Single f_reload_time_pallet_end
	{
		get => _f_reload_time_pallet_end[Index];
		set => _f_reload_time_pallet_end[Index] = value;
	}
	public WeaponCategory f_category
	{
		get => (WeaponCategory) _f_category[Index];
		set => _f_category[Index] = value;
	}
	public WeaponReloadType f_reload_type
	{
		get => (WeaponReloadType) _f_reload_type[Index];
		set => _f_reload_type[Index] = value;
	}
	public System.Int32 f_horde_level
	{
		get => _f_horde_level[Index];
		set => _f_horde_level[Index] = value;
	}
	public System.Int32 f_dm_kill_score
	{
		get => _f_dm_kill_score[Index];
		set => _f_dm_kill_score[Index] = value;
	}
	public System.Boolean f_active
	{
		get => _f_active[Index];
		set => _f_active[Index] = value;
	}
	private static BansheeGz.BGDatabase.BGFieldEntityName _ufle12jhs77_f_name;
	public static BansheeGz.BGDatabase.BGFieldEntityName _f_name => _ufle12jhs77_f_name ?? (_ufle12jhs77_f_name = BGCodeGenUtils.GetField<BansheeGz.BGDatabase.BGFieldEntityName>(MetaDefault, new BGId(5074220435775288759UL, 6103681286880476341UL), () => _ufle12jhs77_f_name = null));
	private static BansheeGz.BGDatabase.BGFieldString _ufle12jhs77_f_display_name;
	public static BansheeGz.BGDatabase.BGFieldString _f_display_name => _ufle12jhs77_f_display_name ?? (_ufle12jhs77_f_display_name = BGCodeGenUtils.GetField<BansheeGz.BGDatabase.BGFieldString>(MetaDefault, new BGId(4955762843518930318UL, 15689018675223417987UL), () => _ufle12jhs77_f_display_name = null));
	private static BansheeGz.BGDatabase.BGFieldInt _ufle12jhs77_f_base_damage;
	public static BansheeGz.BGDatabase.BGFieldInt _f_base_damage => _ufle12jhs77_f_base_damage ?? (_ufle12jhs77_f_base_damage = BGCodeGenUtils.GetField<BansheeGz.BGDatabase.BGFieldInt>(MetaDefault, new BGId(4890035645047449932UL, 3090094229568220042UL), () => _ufle12jhs77_f_base_damage = null));
	private static BansheeGz.BGDatabase.BGFieldFloat _ufle12jhs77_f_range_modifier;
	public static BansheeGz.BGDatabase.BGFieldFloat _f_range_modifier => _ufle12jhs77_f_range_modifier ?? (_ufle12jhs77_f_range_modifier = BGCodeGenUtils.GetField<BansheeGz.BGDatabase.BGFieldFloat>(MetaDefault, new BGId(4683219957302565823UL, 13678527264954651022UL), () => _ufle12jhs77_f_range_modifier = null));
	private static BansheeGz.BGDatabase.BGFieldBool _ufle12jhs77_f_is_semi_auto;
	public static BansheeGz.BGDatabase.BGFieldBool _f_is_semi_auto => _ufle12jhs77_f_is_semi_auto ?? (_ufle12jhs77_f_is_semi_auto = BGCodeGenUtils.GetField<BansheeGz.BGDatabase.BGFieldBool>(MetaDefault, new BGId(4840471375314749571UL, 6716578820605775798UL), () => _ufle12jhs77_f_is_semi_auto = null));
	private static BansheeGz.BGDatabase.BGFieldInt _ufle12jhs77_f_clip_size;
	public static BansheeGz.BGDatabase.BGFieldInt _f_clip_size => _ufle12jhs77_f_clip_size ?? (_ufle12jhs77_f_clip_size = BGCodeGenUtils.GetField<BansheeGz.BGDatabase.BGFieldInt>(MetaDefault, new BGId(5527103663651466065UL, 10923072305087813514UL), () => _ufle12jhs77_f_clip_size = null));
	private static BansheeGz.BGDatabase.BGFieldInt _ufle12jhs77_f_pallet_per_shot;
	public static BansheeGz.BGDatabase.BGFieldInt _f_pallet_per_shot => _ufle12jhs77_f_pallet_per_shot ?? (_ufle12jhs77_f_pallet_per_shot = BGCodeGenUtils.GetField<BansheeGz.BGDatabase.BGFieldInt>(MetaDefault, new BGId(5368421919854404419UL, 1784747819817563808UL), () => _ufle12jhs77_f_pallet_per_shot = null));
	private static BansheeGz.BGDatabase.BGFieldFloat _ufle12jhs77_f_recoil;
	public static BansheeGz.BGDatabase.BGFieldFloat _f_recoil => _ufle12jhs77_f_recoil ?? (_ufle12jhs77_f_recoil = BGCodeGenUtils.GetField<BansheeGz.BGDatabase.BGFieldFloat>(MetaDefault, new BGId(5306463085079295087UL, 6167952524468047003UL), () => _ufle12jhs77_f_recoil = null));
	private static BansheeGz.BGDatabase.BGFieldFloat _ufle12jhs77_f_camera_shake;
	public static BansheeGz.BGDatabase.BGFieldFloat _f_camera_shake => _ufle12jhs77_f_camera_shake ?? (_ufle12jhs77_f_camera_shake = BGCodeGenUtils.GetField<BansheeGz.BGDatabase.BGFieldFloat>(MetaDefault, new BGId(5719258023115973339UL, 11089364542864385422UL), () => _ufle12jhs77_f_camera_shake = null));
	private static BansheeGz.BGDatabase.BGFieldFloat _ufle12jhs77_f_spread;
	public static BansheeGz.BGDatabase.BGFieldFloat _f_spread => _ufle12jhs77_f_spread ?? (_ufle12jhs77_f_spread = BGCodeGenUtils.GetField<BansheeGz.BGDatabase.BGFieldFloat>(MetaDefault, new BGId(5052825553567064992UL, 7269493519236057988UL), () => _ufle12jhs77_f_spread = null));
	private static BansheeGz.BGDatabase.BGFieldFloat _ufle12jhs77_f_spread_move;
	public static BansheeGz.BGDatabase.BGFieldFloat _f_spread_move => _ufle12jhs77_f_spread_move ?? (_ufle12jhs77_f_spread_move = BGCodeGenUtils.GetField<BansheeGz.BGDatabase.BGFieldFloat>(MetaDefault, new BGId(5728888798148707652UL, 11861507793609491092UL), () => _ufle12jhs77_f_spread_move = null));
	private static BansheeGz.BGDatabase.BGFieldFloat _ufle12jhs77_f_draw_time;
	public static BansheeGz.BGDatabase.BGFieldFloat _f_draw_time => _ufle12jhs77_f_draw_time ?? (_ufle12jhs77_f_draw_time = BGCodeGenUtils.GetField<BansheeGz.BGDatabase.BGFieldFloat>(MetaDefault, new BGId(5071738783371732345UL, 8331559641230338984UL), () => _ufle12jhs77_f_draw_time = null));
	private static BansheeGz.BGDatabase.BGFieldFloat _ufle12jhs77_f_shoot_interval;
	public static BansheeGz.BGDatabase.BGFieldFloat _f_shoot_interval => _ufle12jhs77_f_shoot_interval ?? (_ufle12jhs77_f_shoot_interval = BGCodeGenUtils.GetField<BansheeGz.BGDatabase.BGFieldFloat>(MetaDefault, new BGId(5349967516016039171UL, 17838869606908931495UL), () => _ufle12jhs77_f_shoot_interval = null));
	private static BansheeGz.BGDatabase.BGFieldFloat _ufle12jhs77_f_reload_time;
	public static BansheeGz.BGDatabase.BGFieldFloat _f_reload_time => _ufle12jhs77_f_reload_time ?? (_ufle12jhs77_f_reload_time = BGCodeGenUtils.GetField<BansheeGz.BGDatabase.BGFieldFloat>(MetaDefault, new BGId(4900973595100836928UL, 14431906305716465030UL), () => _ufle12jhs77_f_reload_time = null));
	private static BansheeGz.BGDatabase.BGFieldFloat _ufle12jhs77_f_reload_time_pallet_start;
	public static BansheeGz.BGDatabase.BGFieldFloat _f_reload_time_pallet_start => _ufle12jhs77_f_reload_time_pallet_start ?? (_ufle12jhs77_f_reload_time_pallet_start = BGCodeGenUtils.GetField<BansheeGz.BGDatabase.BGFieldFloat>(MetaDefault, new BGId(5558194620638298767UL, 6203067537317166008UL), () => _ufle12jhs77_f_reload_time_pallet_start = null));
	private static BansheeGz.BGDatabase.BGFieldFloat _ufle12jhs77_f_reload_time_pallet_insert;
	public static BansheeGz.BGDatabase.BGFieldFloat _f_reload_time_pallet_insert => _ufle12jhs77_f_reload_time_pallet_insert ?? (_ufle12jhs77_f_reload_time_pallet_insert = BGCodeGenUtils.GetField<BansheeGz.BGDatabase.BGFieldFloat>(MetaDefault, new BGId(4911405929281830021UL, 2494390527557722499UL), () => _ufle12jhs77_f_reload_time_pallet_insert = null));
	private static BansheeGz.BGDatabase.BGFieldFloat _ufle12jhs77_f_reload_time_pallet_end;
	public static BansheeGz.BGDatabase.BGFieldFloat _f_reload_time_pallet_end => _ufle12jhs77_f_reload_time_pallet_end ?? (_ufle12jhs77_f_reload_time_pallet_end = BGCodeGenUtils.GetField<BansheeGz.BGDatabase.BGFieldFloat>(MetaDefault, new BGId(4687962139478950321UL, 17673524966107817662UL), () => _ufle12jhs77_f_reload_time_pallet_end = null));
	private static BansheeGz.BGDatabase.BGFieldEnum _ufle12jhs77_f_category;
	public static BansheeGz.BGDatabase.BGFieldEnum _f_category => _ufle12jhs77_f_category ?? (_ufle12jhs77_f_category = BGCodeGenUtils.GetField<BansheeGz.BGDatabase.BGFieldEnum>(MetaDefault, new BGId(5530380322779058955UL, 2417539453599107204UL), () => _ufle12jhs77_f_category = null));
	private static BansheeGz.BGDatabase.BGFieldEnum _ufle12jhs77_f_reload_type;
	public static BansheeGz.BGDatabase.BGFieldEnum _f_reload_type => _ufle12jhs77_f_reload_type ?? (_ufle12jhs77_f_reload_type = BGCodeGenUtils.GetField<BansheeGz.BGDatabase.BGFieldEnum>(MetaDefault, new BGId(5292676369794396765UL, 3228840254846382227UL), () => _ufle12jhs77_f_reload_type = null));
	private static BansheeGz.BGDatabase.BGFieldInt _ufle12jhs77_f_horde_level;
	public static BansheeGz.BGDatabase.BGFieldInt _f_horde_level => _ufle12jhs77_f_horde_level ?? (_ufle12jhs77_f_horde_level = BGCodeGenUtils.GetField<BansheeGz.BGDatabase.BGFieldInt>(MetaDefault, new BGId(5154641768473479024UL, 4415341554901312417UL), () => _ufle12jhs77_f_horde_level = null));
	private static BansheeGz.BGDatabase.BGFieldInt _ufle12jhs77_f_dm_kill_score;
	public static BansheeGz.BGDatabase.BGFieldInt _f_dm_kill_score => _ufle12jhs77_f_dm_kill_score ?? (_ufle12jhs77_f_dm_kill_score = BGCodeGenUtils.GetField<BansheeGz.BGDatabase.BGFieldInt>(MetaDefault, new BGId(4694662279016321779UL, 11883779653410052013UL), () => _ufle12jhs77_f_dm_kill_score = null));
	private static BansheeGz.BGDatabase.BGFieldBool _ufle12jhs77_f_active;
	public static BansheeGz.BGDatabase.BGFieldBool _f_active => _ufle12jhs77_f_active ?? (_ufle12jhs77_f_active = BGCodeGenUtils.GetField<BansheeGz.BGDatabase.BGFieldBool>(MetaDefault, new BGId(5660476538095711682UL, 17912296700270086335UL), () => _ufle12jhs77_f_active = null));
	public List<Alias_rifegrt_weapon_view_info> Relatedweapon_view_infoListUsingweapon_infoRelation => BGCodeGenUtils.GetRelatedInbound<Alias_rifegrt_weapon_view_info>(Alias_rifegrt_weapon_view_info._f_weapon_info, Id);
	public List<Alias_rifegrt_weapon_monster_info> Relatedweapon_monster_infoListUsingweapon_infoRelation => BGCodeGenUtils.GetRelatedInbound<Alias_rifegrt_weapon_monster_info>(Alias_rifegrt_weapon_monster_info._f_weapon_info, Id);
	private static BansheeGz.BGDatabase.BGIndex _edeht3sdad33_idx_name;
	public static BansheeGz.BGDatabase.BGIndex _idx_name => _edeht3sdad33_idx_name ?? (_edeht3sdad33_idx_name = BGCodeGenUtils.GetIndex(MetaDefault, new BGId(5617466070727969905UL, 2491195634053680003UL), () => _edeht3sdad33_idx_name = null));
	private static readonly E_weapon_info.Factory _factory0_PFS = new E_weapon_info.Factory();
	private static readonly E_weapon_view_info.Factory _factory1_PFS = new E_weapon_view_info.Factory();
	private static readonly E_monster_info.Factory _factory2_PFS = new E_monster_info.Factory();
	private static readonly E_weapon_monster_info.Factory _factory3_PFS = new E_weapon_monster_info.Factory();
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
	public static List<E_weapon_info> FindEntities(Predicate<E_weapon_info> filter, List<E_weapon_info> result=null, Comparison<E_weapon_info> sort=null) => BGCodeGenUtils.FindEntities(MetaDefault, filter, result, sort);
	public static void ForEachEntity(Action<E_weapon_info> action, Predicate<E_weapon_info> filter=null, Comparison<E_weapon_info> sort=null)
	{
		MetaDefault.ForEachEntity(entity => action((E_weapon_info) entity), filter == null ? null : (Predicate<BGEntity>) (entity => filter((E_weapon_info) entity)), sort==null?(Comparison<BGEntity>) null:(e1,e2) => sort((E_weapon_info)e1,(E_weapon_info)e2));
	}
	public static E_weapon_info GetEntity(BGId entityId) => (E_weapon_info) MetaDefault.GetEntity(entityId);
	public static E_weapon_info GetEntity(int index) => (E_weapon_info) MetaDefault[index];
	public static E_weapon_info GetEntity(string entityName) => (E_weapon_info) MetaDefault.GetEntity(entityName);
	public static E_weapon_info NewEntity() => (E_weapon_info) MetaDefault.NewEntity();
	public static E_weapon_info NewEntity(BGId entityId) => (E_weapon_info) MetaDefault.NewEntity(entityId);
	public static E_weapon_info NewEntity(Action<E_weapon_info> callback)
	{
		return (E_weapon_info) MetaDefault.NewEntity(new BGMetaEntity.NewEntityContext(entity => callback((E_weapon_info)entity)));
	}
	public static List<E_weapon_info> FindEntitiesByIndexidx_name(BGIndexOperatorRange<System.String> indexOperator, List<E_weapon_info> result=null) => _idx_name.FindEntitiesByIndex<E_weapon_info>(result, indexOperator);
}

public partial class E_weapon_view_info : BGEntity
{

	public class Factory : BGEntity.EntityFactory
	{
		public BGEntity NewEntity(BGMetaEntity meta) => new E_weapon_view_info(meta);
		public BGEntity NewEntity(BGMetaEntity meta, BGId id) => new E_weapon_view_info(meta, id);
	}
	private static BansheeGz.BGDatabase.BGMetaRow _metaDefault;
	public static BansheeGz.BGDatabase.BGMetaRow MetaDefault => _metaDefault ?? (_metaDefault = BGCodeGenUtils.GetMeta<BansheeGz.BGDatabase.BGMetaRow>(new BGId(4857184795494735401UL,18378752749798393738UL), () => _metaDefault = null));
	public static BansheeGz.BGDatabase.BGRepoEvents Events => BGRepo.I.Events;
	public static int CountEntities => MetaDefault.CountEntities;
	public System.String f_name
	{
		get => _f_name[Index];
		set => _f_name[Index] = value;
	}
	public E_weapon_info f_weapon_info
	{
		get => (E_weapon_info) _f_weapon_info[Index];
		set => _f_weapon_info[Index] = value;
	}
	public UnityEngine.Vector3 f_view_offset
	{
		get => _f_view_offset[Index];
		set => _f_view_offset[Index] = value;
	}
	public System.Boolean f_is_flip
	{
		get => _f_is_flip[Index];
		set => _f_is_flip[Index] = value;
	}
	private static BansheeGz.BGDatabase.BGFieldEntityName _ufle12jhs77_f_name;
	public static BansheeGz.BGDatabase.BGFieldEntityName _f_name => _ufle12jhs77_f_name ?? (_ufle12jhs77_f_name = BGCodeGenUtils.GetField<BansheeGz.BGDatabase.BGFieldEntityName>(MetaDefault, new BGId(5224434718189632410UL, 3597529443704031930UL), () => _ufle12jhs77_f_name = null));
	private static BansheeGz.BGDatabase.BGFieldRelationSingle _ufle12jhs77_f_weapon_info;
	public static BansheeGz.BGDatabase.BGFieldRelationSingle _f_weapon_info => _ufle12jhs77_f_weapon_info ?? (_ufle12jhs77_f_weapon_info = BGCodeGenUtils.GetField<BansheeGz.BGDatabase.BGFieldRelationSingle>(MetaDefault, new BGId(5698069660759478458UL, 14094658840543400859UL), () => _ufle12jhs77_f_weapon_info = null));
	private static BansheeGz.BGDatabase.BGFieldVector3 _ufle12jhs77_f_view_offset;
	public static BansheeGz.BGDatabase.BGFieldVector3 _f_view_offset => _ufle12jhs77_f_view_offset ?? (_ufle12jhs77_f_view_offset = BGCodeGenUtils.GetField<BansheeGz.BGDatabase.BGFieldVector3>(MetaDefault, new BGId(4953629725750552220UL, 16929632367005039238UL), () => _ufle12jhs77_f_view_offset = null));
	private static BansheeGz.BGDatabase.BGFieldBool _ufle12jhs77_f_is_flip;
	public static BansheeGz.BGDatabase.BGFieldBool _f_is_flip => _ufle12jhs77_f_is_flip ?? (_ufle12jhs77_f_is_flip = BGCodeGenUtils.GetField<BansheeGz.BGDatabase.BGFieldBool>(MetaDefault, new BGId(4956780205614061096UL, 3574087736904000444UL), () => _ufle12jhs77_f_is_flip = null));
	private static readonly E_weapon_info.Factory _factory0_PFS = new E_weapon_info.Factory();
	private static readonly E_weapon_view_info.Factory _factory1_PFS = new E_weapon_view_info.Factory();
	private static readonly E_monster_info.Factory _factory2_PFS = new E_monster_info.Factory();
	private static readonly E_weapon_monster_info.Factory _factory3_PFS = new E_weapon_monster_info.Factory();
	private E_weapon_view_info() : base(MetaDefault)
	{
	}
	private E_weapon_view_info(BGId id) : base(MetaDefault, id)
	{
	}
	private E_weapon_view_info(BGMetaEntity meta) : base(meta)
	{
	}
	private E_weapon_view_info(BGMetaEntity meta, BGId id) : base(meta, id)
	{
	}
	public static E_weapon_view_info FindEntity(Predicate<E_weapon_view_info> filter)
	{
		return MetaDefault.FindEntity(entity => filter==null || filter((E_weapon_view_info) entity)) as E_weapon_view_info;
	}
	public static List<E_weapon_view_info> FindEntities(Predicate<E_weapon_view_info> filter, List<E_weapon_view_info> result=null, Comparison<E_weapon_view_info> sort=null) => BGCodeGenUtils.FindEntities(MetaDefault, filter, result, sort);
	public static void ForEachEntity(Action<E_weapon_view_info> action, Predicate<E_weapon_view_info> filter=null, Comparison<E_weapon_view_info> sort=null)
	{
		MetaDefault.ForEachEntity(entity => action((E_weapon_view_info) entity), filter == null ? null : (Predicate<BGEntity>) (entity => filter((E_weapon_view_info) entity)), sort==null?(Comparison<BGEntity>) null:(e1,e2) => sort((E_weapon_view_info)e1,(E_weapon_view_info)e2));
	}
	public static E_weapon_view_info GetEntity(BGId entityId) => (E_weapon_view_info) MetaDefault.GetEntity(entityId);
	public static E_weapon_view_info GetEntity(int index) => (E_weapon_view_info) MetaDefault[index];
	public static E_weapon_view_info GetEntity(string entityName) => (E_weapon_view_info) MetaDefault.GetEntity(entityName);
	public static E_weapon_view_info NewEntity() => (E_weapon_view_info) MetaDefault.NewEntity();
	public static E_weapon_view_info NewEntity(BGId entityId) => (E_weapon_view_info) MetaDefault.NewEntity(entityId);
	public static E_weapon_view_info NewEntity(Action<E_weapon_view_info> callback)
	{
		return (E_weapon_view_info) MetaDefault.NewEntity(new BGMetaEntity.NewEntityContext(entity => callback((E_weapon_view_info)entity)));
	}
}

public partial class E_monster_info : BGEntity
{

	public class Factory : BGEntity.EntityFactory
	{
		public BGEntity NewEntity(BGMetaEntity meta) => new E_monster_info(meta);
		public BGEntity NewEntity(BGMetaEntity meta, BGId id) => new E_monster_info(meta, id);
	}
	private static BansheeGz.BGDatabase.BGMetaRow _metaDefault;
	public static BansheeGz.BGDatabase.BGMetaRow MetaDefault => _metaDefault ?? (_metaDefault = BGCodeGenUtils.GetMeta<BansheeGz.BGDatabase.BGMetaRow>(new BGId(5361719045905385931UL,17304927498360747147UL), () => _metaDefault = null));
	public static BansheeGz.BGDatabase.BGRepoEvents Events => BGRepo.I.Events;
	public static int CountEntities => MetaDefault.CountEntities;
	public System.String f_name
	{
		get => _f_name[Index];
		set => _f_name[Index] = value;
	}
	public System.String f_display_name
	{
		get => _f_display_name[Index];
		set => _f_display_name[Index] = value;
	}
	public System.Int32 f_base_health
	{
		get => _f_base_health[Index];
		set => _f_base_health[Index] = value;
	}
	public System.Single f_move_speed
	{
		get => _f_move_speed[Index];
		set => _f_move_speed[Index] = value;
	}
	public System.Int32 f_max_count
	{
		get => _f_max_count[Index];
		set => _f_max_count[Index] = value;
	}
	/**<summary><![CDATA[
	Start spawning AT the stage
	]]></summary>*/
	public System.Int32 f_start_stage
	{
		get => _f_start_stage[Index];
		set => _f_start_stage[Index] = value;
	}
	/**<summary><![CDATA[
	Stop spawning AFTER the stage
	]]></summary>*/
	public System.Int32 f_end_stage
	{
		get => _f_end_stage[Index];
		set => _f_end_stage[Index] = value;
	}
	private static BansheeGz.BGDatabase.BGFieldEntityName _ufle12jhs77_f_name;
	public static BansheeGz.BGDatabase.BGFieldEntityName _f_name => _ufle12jhs77_f_name ?? (_ufle12jhs77_f_name = BGCodeGenUtils.GetField<BansheeGz.BGDatabase.BGFieldEntityName>(MetaDefault, new BGId(5199085996425251369UL, 1231959888780819631UL), () => _ufle12jhs77_f_name = null));
	private static BansheeGz.BGDatabase.BGFieldString _ufle12jhs77_f_display_name;
	public static BansheeGz.BGDatabase.BGFieldString _f_display_name => _ufle12jhs77_f_display_name ?? (_ufle12jhs77_f_display_name = BGCodeGenUtils.GetField<BansheeGz.BGDatabase.BGFieldString>(MetaDefault, new BGId(5265402778546237615UL, 8582714996709957524UL), () => _ufle12jhs77_f_display_name = null));
	private static BansheeGz.BGDatabase.BGFieldInt _ufle12jhs77_f_base_health;
	public static BansheeGz.BGDatabase.BGFieldInt _f_base_health => _ufle12jhs77_f_base_health ?? (_ufle12jhs77_f_base_health = BGCodeGenUtils.GetField<BansheeGz.BGDatabase.BGFieldInt>(MetaDefault, new BGId(5741010433549342167UL, 6461942175922079874UL), () => _ufle12jhs77_f_base_health = null));
	private static BansheeGz.BGDatabase.BGFieldFloat _ufle12jhs77_f_move_speed;
	public static BansheeGz.BGDatabase.BGFieldFloat _f_move_speed => _ufle12jhs77_f_move_speed ?? (_ufle12jhs77_f_move_speed = BGCodeGenUtils.GetField<BansheeGz.BGDatabase.BGFieldFloat>(MetaDefault, new BGId(4952303815547740193UL, 9340583777696718742UL), () => _ufle12jhs77_f_move_speed = null));
	private static BansheeGz.BGDatabase.BGFieldInt _ufle12jhs77_f_max_count;
	public static BansheeGz.BGDatabase.BGFieldInt _f_max_count => _ufle12jhs77_f_max_count ?? (_ufle12jhs77_f_max_count = BGCodeGenUtils.GetField<BansheeGz.BGDatabase.BGFieldInt>(MetaDefault, new BGId(4691071039300367544UL, 4121527071664136842UL), () => _ufle12jhs77_f_max_count = null));
	private static BansheeGz.BGDatabase.BGFieldInt _ufle12jhs77_f_start_stage;
	public static BansheeGz.BGDatabase.BGFieldInt _f_start_stage => _ufle12jhs77_f_start_stage ?? (_ufle12jhs77_f_start_stage = BGCodeGenUtils.GetField<BansheeGz.BGDatabase.BGFieldInt>(MetaDefault, new BGId(5502007690744568472UL, 12848727859619122086UL), () => _ufle12jhs77_f_start_stage = null));
	private static BansheeGz.BGDatabase.BGFieldInt _ufle12jhs77_f_end_stage;
	public static BansheeGz.BGDatabase.BGFieldInt _f_end_stage => _ufle12jhs77_f_end_stage ?? (_ufle12jhs77_f_end_stage = BGCodeGenUtils.GetField<BansheeGz.BGDatabase.BGFieldInt>(MetaDefault, new BGId(5724066033165208912UL, 3647628603327458214UL), () => _ufle12jhs77_f_end_stage = null));
	private static readonly E_weapon_info.Factory _factory0_PFS = new E_weapon_info.Factory();
	private static readonly E_weapon_view_info.Factory _factory1_PFS = new E_weapon_view_info.Factory();
	private static readonly E_monster_info.Factory _factory2_PFS = new E_monster_info.Factory();
	private static readonly E_weapon_monster_info.Factory _factory3_PFS = new E_weapon_monster_info.Factory();
	private E_monster_info() : base(MetaDefault)
	{
	}
	private E_monster_info(BGId id) : base(MetaDefault, id)
	{
	}
	private E_monster_info(BGMetaEntity meta) : base(meta)
	{
	}
	private E_monster_info(BGMetaEntity meta, BGId id) : base(meta, id)
	{
	}
	public static E_monster_info FindEntity(Predicate<E_monster_info> filter)
	{
		return MetaDefault.FindEntity(entity => filter==null || filter((E_monster_info) entity)) as E_monster_info;
	}
	public static List<E_monster_info> FindEntities(Predicate<E_monster_info> filter, List<E_monster_info> result=null, Comparison<E_monster_info> sort=null) => BGCodeGenUtils.FindEntities(MetaDefault, filter, result, sort);
	public static void ForEachEntity(Action<E_monster_info> action, Predicate<E_monster_info> filter=null, Comparison<E_monster_info> sort=null)
	{
		MetaDefault.ForEachEntity(entity => action((E_monster_info) entity), filter == null ? null : (Predicate<BGEntity>) (entity => filter((E_monster_info) entity)), sort==null?(Comparison<BGEntity>) null:(e1,e2) => sort((E_monster_info)e1,(E_monster_info)e2));
	}
	public static E_monster_info GetEntity(BGId entityId) => (E_monster_info) MetaDefault.GetEntity(entityId);
	public static E_monster_info GetEntity(int index) => (E_monster_info) MetaDefault[index];
	public static E_monster_info GetEntity(string entityName) => (E_monster_info) MetaDefault.GetEntity(entityName);
	public static E_monster_info NewEntity() => (E_monster_info) MetaDefault.NewEntity();
	public static E_monster_info NewEntity(BGId entityId) => (E_monster_info) MetaDefault.NewEntity(entityId);
	public static E_monster_info NewEntity(Action<E_monster_info> callback)
	{
		return (E_monster_info) MetaDefault.NewEntity(new BGMetaEntity.NewEntityContext(entity => callback((E_monster_info)entity)));
	}
}

public partial class E_weapon_monster_info : BGEntity
{

	public class Factory : BGEntity.EntityFactory
	{
		public BGEntity NewEntity(BGMetaEntity meta) => new E_weapon_monster_info(meta);
		public BGEntity NewEntity(BGMetaEntity meta, BGId id) => new E_weapon_monster_info(meta, id);
	}
	private static BansheeGz.BGDatabase.BGMetaRow _metaDefault;
	public static BansheeGz.BGDatabase.BGMetaRow MetaDefault => _metaDefault ?? (_metaDefault = BGCodeGenUtils.GetMeta<BansheeGz.BGDatabase.BGMetaRow>(new BGId(5250136442147556947UL,15076128842491762841UL), () => _metaDefault = null));
	public static BansheeGz.BGDatabase.BGRepoEvents Events => BGRepo.I.Events;
	public static int CountEntities => MetaDefault.CountEntities;
	public System.String f_name
	{
		get => _f_name[Index];
		set => _f_name[Index] = value;
	}
	public E_weapon_info f_weapon_info
	{
		get => (E_weapon_info) _f_weapon_info[Index];
		set => _f_weapon_info[Index] = value;
	}
	public System.Int32 f_level
	{
		get => _f_level[Index];
		set => _f_level[Index] = value;
	}
	public System.Int32 f_override_damage
	{
		get => _f_override_damage[Index];
		set => _f_override_damage[Index] = value;
	}
	private static BansheeGz.BGDatabase.BGFieldEntityName _ufle12jhs77_f_name;
	public static BansheeGz.BGDatabase.BGFieldEntityName _f_name => _ufle12jhs77_f_name ?? (_ufle12jhs77_f_name = BGCodeGenUtils.GetField<BansheeGz.BGDatabase.BGFieldEntityName>(MetaDefault, new BGId(5127487267876670678UL, 14368107649059033257UL), () => _ufle12jhs77_f_name = null));
	private static BansheeGz.BGDatabase.BGFieldRelationSingle _ufle12jhs77_f_weapon_info;
	public static BansheeGz.BGDatabase.BGFieldRelationSingle _f_weapon_info => _ufle12jhs77_f_weapon_info ?? (_ufle12jhs77_f_weapon_info = BGCodeGenUtils.GetField<BansheeGz.BGDatabase.BGFieldRelationSingle>(MetaDefault, new BGId(5508854425870387259UL, 9280994771638243224UL), () => _ufle12jhs77_f_weapon_info = null));
	private static BansheeGz.BGDatabase.BGFieldInt _ufle12jhs77_f_level;
	public static BansheeGz.BGDatabase.BGFieldInt _f_level => _ufle12jhs77_f_level ?? (_ufle12jhs77_f_level = BGCodeGenUtils.GetField<BansheeGz.BGDatabase.BGFieldInt>(MetaDefault, new BGId(4664274901627454477UL, 5048634251957559695UL), () => _ufle12jhs77_f_level = null));
	private static BansheeGz.BGDatabase.BGFieldInt _ufle12jhs77_f_override_damage;
	public static BansheeGz.BGDatabase.BGFieldInt _f_override_damage => _ufle12jhs77_f_override_damage ?? (_ufle12jhs77_f_override_damage = BGCodeGenUtils.GetField<BansheeGz.BGDatabase.BGFieldInt>(MetaDefault, new BGId(5702164298426902775UL, 1384047226329852071UL), () => _ufle12jhs77_f_override_damage = null));
	private static readonly E_weapon_info.Factory _factory0_PFS = new E_weapon_info.Factory();
	private static readonly E_weapon_view_info.Factory _factory1_PFS = new E_weapon_view_info.Factory();
	private static readonly E_monster_info.Factory _factory2_PFS = new E_monster_info.Factory();
	private static readonly E_weapon_monster_info.Factory _factory3_PFS = new E_weapon_monster_info.Factory();
	private E_weapon_monster_info() : base(MetaDefault)
	{
	}
	private E_weapon_monster_info(BGId id) : base(MetaDefault, id)
	{
	}
	private E_weapon_monster_info(BGMetaEntity meta) : base(meta)
	{
	}
	private E_weapon_monster_info(BGMetaEntity meta, BGId id) : base(meta, id)
	{
	}
	public static E_weapon_monster_info FindEntity(Predicate<E_weapon_monster_info> filter)
	{
		return MetaDefault.FindEntity(entity => filter==null || filter((E_weapon_monster_info) entity)) as E_weapon_monster_info;
	}
	public static List<E_weapon_monster_info> FindEntities(Predicate<E_weapon_monster_info> filter, List<E_weapon_monster_info> result=null, Comparison<E_weapon_monster_info> sort=null) => BGCodeGenUtils.FindEntities(MetaDefault, filter, result, sort);
	public static void ForEachEntity(Action<E_weapon_monster_info> action, Predicate<E_weapon_monster_info> filter=null, Comparison<E_weapon_monster_info> sort=null)
	{
		MetaDefault.ForEachEntity(entity => action((E_weapon_monster_info) entity), filter == null ? null : (Predicate<BGEntity>) (entity => filter((E_weapon_monster_info) entity)), sort==null?(Comparison<BGEntity>) null:(e1,e2) => sort((E_weapon_monster_info)e1,(E_weapon_monster_info)e2));
	}
	public static E_weapon_monster_info GetEntity(BGId entityId) => (E_weapon_monster_info) MetaDefault.GetEntity(entityId);
	public static E_weapon_monster_info GetEntity(int index) => (E_weapon_monster_info) MetaDefault[index];
	public static E_weapon_monster_info GetEntity(string entityName) => (E_weapon_monster_info) MetaDefault.GetEntity(entityName);
	public static E_weapon_monster_info NewEntity() => (E_weapon_monster_info) MetaDefault.NewEntity();
	public static E_weapon_monster_info NewEntity(BGId entityId) => (E_weapon_monster_info) MetaDefault.NewEntity(entityId);
	public static E_weapon_monster_info NewEntity(Action<E_weapon_monster_info> callback)
	{
		return (E_weapon_monster_info) MetaDefault.NewEntity(new BGMetaEntity.NewEntityContext(entity => callback((E_weapon_monster_info)entity)));
	}
}
#pragma warning restore 414
