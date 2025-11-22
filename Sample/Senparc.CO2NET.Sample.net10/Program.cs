using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Senparc.CO2NET.AspNet;
using Senparc.CO2NET.Helpers;
using Senparc.CO2NET.RegisterServices;
using Senparc.CO2NET.Sample.net10.Services;
using Senparc.CO2NET.WebApi;
using Senparc.CO2NET.WebApi.WebApiEngines;
using Senparc.CO2NET;
using Microsoft.Extensions.Options;
using Senparc.CO2NET.Cache;
using Senparc.CO2NET.Cache.Memcached;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var mvpBuilder = builder.Services.AddControllersWithViews();

//ʹ�ñ��ػ����������
builder.Services.AddMemoryCache();

#region ����ȫ�����ã�һ�д��룩

//Senparc.Weixin ע�ᣨ���룩
builder.Services.AddSenparcGlobalServices(builder.Configuration);

#endregion


#region WebApiEngine����ѡ��

//���Բ��ԣ�ע�͵����´���󣬿ɿ���΢�Ź��ں�SDK�ӿڼ�ע����Ϣ
Senparc.CO2NET.WebApi.Register.OmitCategoryList.Add(Senparc.NeuChar.PlatformType.WeChat_OfficialAccount.ToString());

//�������Ӳ���
Senparc.CO2NET.WebApi.Register.AdditionalClasses.Add(typeof(AdditionalType), "Additional");
Senparc.CO2NET.WebApi.Register.AdditionalMethods.Add(typeof(AdditionalMethod).GetMethod("TestApi"), "Additional");
Senparc.CO2NET.WebApi.Register.AdditionalMethods.Add(typeof(EncryptHelper).GetMethod("GetMD5", new[] { typeof(string), typeof(string) }), "Additional");

var docXmlPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "ApiDocXml");
builder.Services.AddAndInitDynamicApi(mvpBuilder, options =>
{
    options.DocXmlPath = docXmlPath;
    options.DefaultRequestMethod = ApiRequestMethod.Get;
    options.BaseApiControllerType = null;
    options.CopyCustomAttributes = true;
    options.TaskCount = Environment.ProcessorCount * 4;
    options.ShowDetailApiLog = true;
    options.AdditionalAttributeFunc = null;
    options.ForbiddenExternalAccess = true;
});

#endregion

var app = builder.Build();

#region �������ã�һ����룩

//�ֶ���ȡ������Ϣ��ʹ�����·���
var senparcSetting = app.Services.GetService<IOptions<SenparcSetting>>()!.Value;

//����΢�����ã����룩
var registerService = app.UseSenparcGlobal(app.Environment,
    senparcSetting /* ��Ϊ null �򸲸� appsettings  �е� SenpacSetting ����*/,
    register =>
    {
        #region CO2NET ȫ������

        #region ȫ�ֻ������ã����裩

        //��ͬһ���ֲ�ʽ����ͬʱ�����ڶ����վ��Ӧ�ó���أ�ʱ������ʹ�������ռ佫����루�Ǳ��룩
        register.ChangeDefaultCacheNamespace("CO2NETCache.net10.0");

        #region ���ú�ʹ�� Redis

        //����ȫ��ʹ��Redis���棨���裬������
        var redisConfigurationStr = senparcSetting.Cache_Redis_Configuration;
        var useRedis = !string.IsNullOrEmpty(redisConfigurationStr) && redisConfigurationStr != "Redis����";
        if (useRedis)//����Ϊ�˷��㲻ͬ�����Ŀ����߽������ã��������жϵķ�ʽ��ʵ�ʿ�������һ����ȷ���ģ������if�������Ժ���
        {
            /* ˵����
             * 1��Redis �������ַ�����Ϣ��� Config.SenparcSetting.Cache_Redis_Configuration �Զ���ȡ��ע�ᣬ�粻��Ҫ�޸ģ��·��������Ժ���
            /* 2�������ֶ��޸ģ�����ͨ���·� SetConfigurationOption �����ֶ����� Redis ������Ϣ�����޸����ã����������ã�
             */
            Senparc.CO2NET.Cache.CsRedis.Register.SetConfigurationOption(redisConfigurationStr);

            //���»�������ȫ�ֻ�������Ϊ Redis
            Senparc.CO2NET.Cache.CsRedis.Register.UseKeyValueRedisNow();//��ֵ�Ի�����ԣ��Ƽ���
                                                                        //Senparc.CO2NET.Cache.Redis.Register.UseHashRedisNow();//HashSet�����ʽ�Ļ������

            //Ҳ����ͨ�����·�ʽ�Զ��嵱ǰ��Ҫ���õĻ������
            //CacheStrategyFactory.RegisterObjectCacheStrategy(() => RedisObjectCacheStrategy.Instance);//��ֵ��
            //CacheStrategyFactory.RegisterObjectCacheStrategy(() => RedisHashSetObjectCacheStrategy.Instance);//HashSet
        }
        //������ﲻ����Redis�������ã���Ŀǰ����Ĭ��ʹ���ڴ滺�� 

        #endregion

        #region ���ú�ʹ�� Memcached

        //����Memcached���棨���裬������
        var memcachedConfigurationStr = senparcSetting.Cache_Memcached_Configuration;
        var useMemcached = !string.IsNullOrEmpty(memcachedConfigurationStr) && memcachedConfigurationStr != "Memcached����";

        if (useMemcached) //����Ϊ�˷��㲻ͬ�����Ŀ����߽������ã��������жϵķ�ʽ��ʵ�ʿ�������һ����ȷ���ģ������if�������Ժ���
        {
            app.UseEnyimMemcached();

            /* ˵����
            * 1��Memcached �������ַ�����Ϣ��� Config.SenparcSetting.Cache_Memcached_Configuration �Զ���ȡ��ע�ᣬ�粻��Ҫ�޸ģ��·��������Ժ���
           /* 2�������ֶ��޸ģ�����ͨ���·� SetConfigurationOption �����ֶ����� Memcached ������Ϣ�����޸����ã����������ã�
            */
            Senparc.CO2NET.Cache.Memcached.Register.SetConfigurationOption(redisConfigurationStr);

            //���»�������ȫ�ֻ�������Ϊ Memcached
            Senparc.CO2NET.Cache.Memcached.Register.UseMemcachedNow();

            //Ҳ����ͨ�����·�ʽ�Զ��嵱ǰ��Ҫ���õĻ������
            CacheStrategyFactory.RegisterObjectCacheStrategy(() => MemcachedObjectCacheStrategy.Instance);
        }

        #endregion

        #endregion

        #region ע����־�����裬���飩

        register.RegisterTraceLog(ConfigTraceLog);//����TraceLog

        #endregion

        #endregion
    },

#region ɨ���Զ�����չ����

    //�Զ�ɨ���Զ�����չ���棨��ѡһ��
    autoScanExtensionCacheStrategies: true //Ĭ��Ϊ true�����Բ�����
                                           //ָ���Զ�����չ���棨��ѡһ��
                                           //autoScanExtensionCacheStrategies: false, extensionCacheStrategiesFunc: () => GetExCacheStrategies(senparcSetting.Value)

#endregion
            );

#endregion


/// <summary>
/// ����ȫ�ָ�����־
/// </summary>
void ConfigTraceLog()
{
    //������ΪDebug״̬ʱ��/App_Data/SenparcTraceLog/Ŀ¼�»�������־�ļ���¼���е�API������־����ʽ�����汾����ر�

    //���ȫ�ֵ�IsDebug��Senparc.CO2NET.Config.IsDebug��Ϊfalse���˴����Ե�������true�������Զ�Ϊtrue
    Senparc.CO2NET.Trace.SenparcTrace.SendCustomLog("ϵͳ��־", "ϵͳ����");//ֻ��Senparc.CO2NET.Config.IsDebug = true���������Ч

    //ȫ���Զ�����־��¼�ص�
    Senparc.CO2NET.Trace.SenparcTrace.OnLogFunc = () =>
    {
        //����ÿ�δ���Log����Ҫִ�еĴ���
    };

    Senparc.CO2NET.Trace.SenparcTrace.OnBaseExceptionFunc = ex =>
    {
        //����ÿ�δ���BaseException����Ҫִ�еĴ���
    };
}
