using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web.Services.Description;
using System.Xml.Serialization;

namespace ClientTest
{
    public class WSHelper
    {
        /// <summary>
        /// 输出的dll文件名称
        /// </summary>
        private static string m_OutputDllFilename;

        /// <summary>
        /// WebService代理类名称
        /// </summary>
        private static string m_ProxyClassName;

        /// <summary>
        /// WebService代理类实例
        /// </summary>
        private static object m_ObjInvoke;

        /// <summary>
        /// 接口方法字典
        /// </summary>
        private static Dictionary<EMethod, MethodInfo> m_MethodDic = new Dictionary<EMethod, MethodInfo>();

        /// <summary>
        /// 创建WebService，生成客户端代理程序集文件
        /// </summary>
        /// <param name="error">错误信息</param>
        /// <returns>返回：true或false</returns>
        public static bool CreateWebService(out string error)
        {
            try
            {
                error = string.Empty;
                m_OutputDllFilename = ConfigurationManager.AppSettings["OutputDllFilename"];
                m_ProxyClassName = ConfigurationManager.AppSettings["ProxyClassName"];
                string webServiceUrl = ConfigurationManager.AppSettings["WebServiceUrl"];
                webServiceUrl += "?WSDL";

                // 如果程序集已存在，直接使用
                if (File.Exists(Path.Combine(Environment.CurrentDirectory, m_OutputDllFilename)))
                {
                    BuildMethods();
                    return true;
                }

                //使用 WebClient 下载 WSDL 信息。
                WebClient web = new WebClient();
                Stream stream = web.OpenRead(webServiceUrl);

                //创建和格式化 WSDL 文档。
                if (stream != null)
                {
                    // 格式化WSDL
                    ServiceDescription description = ServiceDescription.Read(stream);

                    // 创建客户端代理类。
                    ServiceDescriptionImporter importer = new ServiceDescriptionImporter
                    {
                        ProtocolName = "Soap",
                        Style = ServiceDescriptionImportStyle.Client,
                        CodeGenerationOptions =
                            CodeGenerationOptions.GenerateProperties | CodeGenerationOptions.GenerateNewAsync
                    };

                    // 添加 WSDL 文档。
                    importer.AddServiceDescription(description, null, null);

                    //使用 CodeDom 编译客户端代理类。
                    CodeNamespace nmspace = new CodeNamespace();
                    CodeCompileUnit unit = new CodeCompileUnit();
                    unit.Namespaces.Add(nmspace);

                    ServiceDescriptionImportWarnings warning = importer.Import(nmspace, unit);
                    CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");

                    CompilerParameters parameter = new CompilerParameters
                    {
                        GenerateExecutable = false,
                        // 指定输出dll文件名。
                        OutputAssembly = m_OutputDllFilename
                    };

                    parameter.ReferencedAssemblies.Add("System.dll");
                    parameter.ReferencedAssemblies.Add("System.XML.dll");
                    parameter.ReferencedAssemblies.Add("System.Web.Services.dll");
                    parameter.ReferencedAssemblies.Add("System.Data.dll");

                    // 编译输出程序集
                    CompilerResults result = provider.CompileAssemblyFromDom(parameter, unit);

                    // 使用 Reflection 调用 WebService。
                    if (!result.Errors.HasErrors)
                    {
                        BuildMethods();
                        return true;
                    }
                    else
                    {
                        error = "反射生成dll文件时异常";
                    }
                    stream.Close();
                    stream.Dispose();
                }
                else
                {
                    error = "打开WebServiceUrl失败";
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }
            return false;
        }

        /// <summary>
        /// 反射构建Methods
        /// </summary>
        private static void BuildMethods()
        {
            Assembly asm = Assembly.LoadFrom(m_OutputDllFilename);
            //var types = asm.GetTypes();
            Type asmType = asm.GetType(m_ProxyClassName);
            m_ObjInvoke = Activator.CreateInstance(asmType);

            //var methods = asmType.GetMethods();
            var methods = Enum.GetNames(typeof(EMethod)).ToList();
            foreach (var item in methods)
            {
                var methodInfo = asmType.GetMethod(item);
                if (methodInfo != null)
                {
                    var method = (EMethod)Enum.Parse(typeof(EMethod), item);
                    m_MethodDic.Add(method, methodInfo);
                }
            }
        }

        /// <summary>
        /// 获取请求响应
        /// </summary>
        /// <param name="method">方法</param>
        /// <param name="para">参数</param>
        /// <returns>返回：Json串</returns>
        public static string GetResponseString(EMethod method, params object[] para)
        {
            string result = null;
            if (m_MethodDic.ContainsKey(method))
            {
                var temp = m_MethodDic[method].Invoke(m_ObjInvoke, para);
                if (temp != null)
                {
                    result = temp.ToString();
                }
            }
            return result;
        }
    }
}
