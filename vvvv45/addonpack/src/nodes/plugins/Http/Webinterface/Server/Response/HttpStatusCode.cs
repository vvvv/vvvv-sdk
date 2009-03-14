using System;
using System.Collections.Generic;
using System.Text;
using VVVV.Webinterface.Utilities;
using System.Diagnostics;

namespace VVVV.Webinterface.HttpServer
{

    /// <summary>
    /// Contains all Statuscodes which are defined for hte HTTP 1.1 Standrad by the W3C 
    /// </summary>
    /// <see cref="http://www.w3.org/Protocols/rfc2616/rfc2616-sec10.html"/>
    /// </summary>
    struct HTTPStatusCode
    {

        #region field declaration 

        // Information 3xx
        public string Code100;
        public string Code101;


        //Successful 2XX
        public string Code200;
        public string Code201;
        public string Code202;
        public string Code203;
        public string Code204;
        public string Code205;
        public string Code206;

        //Redirection 3xx
        public string Code300;
        public string Code301;
        public string Code302;
        public string Code303;
        public string Code304;
        public string Code305;
        public string Code306;
        public string Code307;

        //Client Error 4xx
        public string Code400;
        public string Code401;
        public string Code402;
        public string Code403;
        public string Code404;
        public string Code405;
        public string Code406;
        public string Code407;
        public string Code408;
        public string Code409;
        public string Code410;
        public string Code411;
        public string Code412;
        public string Code413;
        public string Code414;
        public string Code415;
        public string Code416;
        public string Code417;
        

        //Server Error 5xx
        public string Code500;
        public string Code501;
        public string Code502;
        public string Code503;
        public string Code504;
        public string Code505;
         
        //Own Error Codes 6xx
        public string Code600;
        public string Code606;

        #endregion  field declaration


        /// <summary>
        /// creates a struct with all Http 1.1 Status Codes
        /// <see cref="http://www.w3.org/Protocols/rfc2616/rfc2616-sec10.html"/>
        /// </summary>
        /// <param name="pStatusCodeText">Sets the StatusCode 606</param>
        public HTTPStatusCode(string pStatusCodeText)
        {

            this.Code100 = "100 Continue";
            this.Code101 = "101 Switching Protocols" ;


            //Successful 2XX
            this.Code200 =  "200 OK" ;
            this.Code201 =  "201Created" ;
            this.Code202 =  "202 Accepted" ;
            this.Code203 =  "203 Non-Authoritative Information" ;
            this.Code204 =  "204 No Content" ;
            this.Code205 =  "205 Resent Content" ;
            this.Code206=  "206 Partial Content" ;

            //Redirection 3xx
            this.Code300=  "300 Mulitple Choices" ;
            this.Code301=  "301 Moved Permanently" ;
            this.Code302=  "302 Found" ;
            this.Code303=  "303 See Other" ;
            this.Code304 =  "304 Not Modified" ;
            this.Code305 =  "305 See Other" ;
            this.Code306 =  "306 Unused" ;
            this.Code307 =  "307 Temporary Redirect" ;

            //Client Error 4xx
            this.Code400=  "400 Bad Request" ;
            this.Code401=  "401 Unauthorized" ;
            this.Code402=  "402 Payment Required" ;
            this.Code403=  "403 Forbidden" ;
            this.Code404=  "404 Not Found" ;
            this.Code405=  "405 Method Not Allowed" ;
            this.Code406=  "406 Not Acceptable" ;
            this.Code407=  "407 Proxy Authentication Required" ;
            this.Code408=  "408 Request Timeout" ;
            this.Code409=  "409 Confilct" ;
            this.Code410=  "410 Gone" ;
            this.Code411=  "411 Length Required" ;
            this.Code412=  "412 Precondition Failed" ;
            this.Code413=  "413 Request Entity Too Large" ;
            this.Code414=  "414 Request URI Too Long" ;
            this.Code415=  "415 Unsupported Media Type" ;
            this.Code416=  "416 Requested Range Not Satisfiable" ;
            this.Code417=  "417 Expextion Faild" ;
            

            //Server Error 5xx
            this.Code500=  "500 Inernal Server Error" ;
            this.Code501=  "501 Not Implemented" ;
            this.Code502=  "502 Bad Gateway" ;
            this.Code503=  "503 Service Unavailable" ;
            this.Code504=  "504 Gateway Timeout" ;
            this.Code505=  "505 HTTP Version Nnot Supported" ;
             
            //Own Error Codes 6xx
            this.Code600=  "600 Error occured in VVVV" ;
            this.Code606 = "606 " +pStatusCodeText;
        }

       

    }
}
