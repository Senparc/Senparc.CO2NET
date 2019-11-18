<img src="https://sdk.weixin.senparc.com/images/senparc-logo-500.jpg" /> 

# Senparc.CO2NET
[![Build status](https://mysenparc.visualstudio.com/Senparc%20SDK/_apis/build/status/CO2NET/Senparc.CO2NET%20-ASP.NET%20Core-CI-clone)](https://mysenparc.visualstudio.com/Senparc%20SDK/_build/latest?definitionId=11)
[![Build status](https://ci.appveyor.com/api/projects/status/uqhyn9i2x5r300dq/branch/master?svg=true)](https://ci.appveyor.com/project/JeffreySu/senparc-co2net/branch/master)
[![NuGet](https://img.shields.io/nuget/dt/Senparc.CO2NET.svg)](https://www.nuget.org/packages/Senparc.CO2NET)

| 模块功能    |        Nuget 名称          |  Nuget                                                                                | 支持 .NET 版本 
|------------|----------------------------|---------------------------------------------------------------------------------------|--------------------------------------
| CO2NET 基础库 | Senparc.CO2NET   | [![Senparc.CO2NET][1.1]][1.2]    [![Senparc.CO2NET][nuget-img-base]][nuget-url-base]  |  ![.NET 3.5][net35Y]    ![.NET 4.0][net40Y]   ![.NET 4.5][net45Y]    ![.NET Core 2.0][core20Y]
| APM 模块 | Senparc.CO2NET.APM   | [![Senparc.CO2NET.APM][2.1]][2.2]    [![Senparc.CO2NET.APM][nuget-img-base-apm]][nuget-url-base-apm]  |  ![.NET 3.5][net35Y]    ![.NET 4.0][net40Y]   ![.NET 4.5][net45Y]    ![.NET Core 2.0][core20Y]
| Redis 基础库 | Senparc.CO2NET.Cache.Redis   | [![Senparc.CO2NET.Cache.Redis][3.1]][3.2]    [![Senparc.CO2NET.Cache.Redis][nuget-img-base-redis]][nuget-url-base-redis]  |  ![.NET 3.5][net35N]    ![.NET 4.0][net40N]   ![.NET 4.5][net45Y]    ![.NET Core 2.0][core20Y]
| Memcached 基础库 | Senparc.CO2NET.Cache.Memcached   | [![Senparc.CO2NET.Cache.Memcached][4.1]][4.2]    [![Senparc.CO2NET.Cache.Memcached][nuget-img-base-memcached]][nuget-url-base-memcached]  |  ![.NET 3.5][net35N]    ![.NET 4.0][net40N]   ![.NET 4.5][net45Y]    ![.NET Core 2.0][core20Y]

![.NET Core 2.0][core20Y] 同时支持 .NET Standard 2.0+ 及 .NET Core 2.2+

> CO2NET 将逐步停止对 .NET Framework 4.0 及以下版本的支持。

[1.1]: https://img.shields.io/nuget/v/Senparc.CO2NET.svg?style=flat
[1.2]: https://www.nuget.org/packages/Senparc.CO2NET
[2.1]: https://img.shields.io/nuget/v/Senparc.CO2NET.APM.svg?style=flat
[2.2]: https://www.nuget.org/packages/Senparc.CO2NET.APM
[3.1]: https://img.shields.io/nuget/v/Senparc.CO2NET.Cache.Redis.svg?style=flat
[3.2]: https://www.nuget.org/packages/Senparc.CO2NET.Cache.Redis
[4.1]: https://img.shields.io/nuget/v/Senparc.CO2NET.Cache.Memcached.svg?style=flat
[4.2]: https://www.nuget.org/packages/Senparc.CO2NET.Cache.Memcached

[net35Y]: https://img.shields.io/badge/3.5-Y-brightgreen.svg
[net35N]: https://img.shields.io/badge/3.5-N-lightgrey.svg
[net40Y]: https://img.shields.io/badge/4.0-Y-brightgreen.svg
[net40N]: https://img.shields.io/badge/4.0-N-lightgrey.svg
[net40N-]: https://img.shields.io/badge/4.0----lightgrey.svg
[net45Y]: https://img.shields.io/badge/4.5-Y-brightgreen.svg
[net45N]: https://img.shields.io/badge/4.5-N-lightgrey.svg
[net45N-]: https://img.shields.io/badge/4.5----lightgrey.svg
[net461Y]: https://img.shields.io/badge/4.6.1-Y-brightgreen.svg
[net461N]: https://img.shields.io/badge/4.6.1-N-lightgrey.svg
[coreY]: https://img.shields.io/badge/standard2.0-Y-brightgreen.svg
[coreN]: https://img.shields.io/badge/standard2.0-N-lightgrey.svg
[coreN-]: https://img.shields.io/badge/standard2.0----lightgrey.svg
[core20Y]: https://img.shields.io/badge/standard2.0-Y-brightgreen.svg
[core20N]: https://img.shields.io/badge/standard2.0-N-lightgrey.svg

[nuget-img-base]: https://img.shields.io/nuget/dt/Senparc.CO2NET.svg
[nuget-url-base]: https://www.nuget.org/packages/Senparc.CO2NET
[nuget-img-base-apm]: https://img.shields.io/nuget/dt/Senparc.CO2NET.APM.svg
[nuget-url-base-apm]: https://www.nuget.org/packages/Senparc.CO2NET.APM
[nuget-img-base-redis]: https://img.shields.io/nuget/dt/Senparc.CO2NET.Cache.Redis.svg
[nuget-url-base-redis]: https://www.nuget.org/packages/Senparc.CO2NET.Cache.Redis
[nuget-img-base-memcached]: https://img.shields.io/nuget/dt/Senparc.CO2NET.Cache.Memcached.svg
[nuget-url-base-memcached]: https://www.nuget.org/packages/Senparc.CO2NET.Cache.Memcached

Senparc.CO2NET 是一个支持 .NET Framework 和 .NET Core 的公共基础扩展库，包含常规开发所需要的基础帮助类。

开发者可以直接使用 CO2NET 为项目提供公共基础方法，免去重复准备和维护公共代码的痛苦。

Senparc.CO2NET 已经作为 [Senparc.Weixin SDK](https://github.com/JeffreySu/WeiXinMPSDK)、[SCF](https://github.com/SenparcCoreFramework/SCF) 等 Senparc 系列产品的的基础库被依赖。

## 如何使用 Nuget 安装？

* CO2NET Nuget 地址：https://www.nuget.org/packages/Senparc.CO2NET
* 命令：
```
PM> Install-Package Senparc.CO2NET
```

## 阶段

目前发布的已经是稳定版，持续更新中，您可关注本项目进展！

程序集在线文档：<a href="http://doc.weixin.senparc.com/html/G_Senparc_CO2NET.htm" target="_blank">http://doc.weixin.senparc.com/html/G_Senparc_CO2NET.htm</a>

本项目后期将会配备更加完整的文档，如果大家迫不及待想要尝试，可以打开解决方案文件，参考单元测试项目（Senparc.CO2NET.Tests），每一个方法都能找到对应的用法，本项目完整指之一就是将单元测试代码覆盖率做到接近100%。

## 视频预告介绍
[抢先预览（2018年6月15日）](http://study.163.com/course/courseLearn.htm?courseId=1004873017&share=2&shareId=400000000353002#/learn/video?lessonId=1052874494&courseId=1004873017)

[使用 CO2NET 初始化微信项目及普通项目（2018年6月22日）](http://study.163.com/course/courseLearn.htm?courseId=1004873017&share=2&shareId=400000000353002#/learn/video?lessonId=1052903157&courseId=1004873017)
