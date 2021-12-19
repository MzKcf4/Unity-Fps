using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitInfoDto
{
    public List<HitEntityInfoDto> hitEntityInfoDtoList;
    public List<HitWallInfoDto> hitWallInfoDtoList;
    
    public HitInfoDto()
    {}
    
    public HitInfoDto(List<HitEntityInfoDto> hitEntityInfoDtoList, List<HitWallInfoDto> hitWallInfoDtoList)
    {
        this.hitEntityInfoDtoList = hitEntityInfoDtoList;
        this.hitWallInfoDtoList = hitWallInfoDtoList;
    }
}
