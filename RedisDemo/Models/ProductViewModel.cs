/* ==============================================================================
* 命名空间：RedisDemo.Models 
* 类 名 称：ProductViewModel
* 创 建 者：Qing
* 创建时间：2019/04/07 16:53:14
* CLR 版本：4.0.30319.42000
* 保存的文件名：ProductViewModel
* 文件版本：V1.0.0.0
*
* 功能描述：N/A 
*
* 修改历史：
*
*
* ==============================================================================
*         CopyRight @ 班纳工作室 2019. All rights reserved
* ==============================================================================*/

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using RedisDemo.Data;
using StackExchange.Redis;

namespace RedisDemo.Models
{
    public class ProductViewModel
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 产品名称
        /// </summary>
        [Display(Name="产品名称")]
        public string Name { get; set; }
        /// <summary>
        /// 产品图片
        /// </summary>
        [Display(Name="产品图片")]
        public string Url { get; set; }
        /// <summary>
        /// 浏览量
        /// </summary>
        [Display(Name="浏览量")]
        public string ViewCount { get; set; }
    }
}
