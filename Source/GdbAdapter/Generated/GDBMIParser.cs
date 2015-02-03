// $ANTLR 3.1.2 GDBMI.g 2011-06-23 02:29:56

// The variable 'variable' is assigned but its value is never used.
#pragma warning disable 168, 219
// Unreachable code detected.
#pragma warning disable 162

		
	using System.Collections.Generic;
	//using Logging;


using System;
using Antlr.Runtime;
using IList 		= System.Collections.IList;
using ArrayList 	= System.Collections.ArrayList;
using Stack 		= Antlr.Runtime.Collections.StackList;


namespace  GdbAdapter.Parsers 
{
public partial class GDBMIParser : Parser
{
    public static readonly string[] tokenNames = new string[] 
	{
        "<invalid>", 
		"<EOR>", 
		"<DOWN>", 
		"<UP>", 
		"ASYNC_CLASS", 
		"C_STRING", 
		"COMMA", 
		"CONSOLE", 
		"EOM", 
		"EXEC", 
		"LOG", 
		"NL", 
		"NOTIFY", 
		"RESULT", 
		"RESULT_CLASS", 
		"STATUS", 
		"STRING", 
		"TARGET", 
		"TOKEN", 
		"WS", 
		"'['", 
		"'[]'", 
		"']'", 
		"'{'", 
		"'{}'", 
		"'}'"
    };

    public const int EOF = -1;
    public const int T__20 = 20;
    public const int T__21 = 21;
    public const int T__22 = 22;
    public const int T__23 = 23;
    public const int T__24 = 24;
    public const int T__25 = 25;
    public const int ASYNC_CLASS = 4;
    public const int C_STRING = 5;
    public const int COMMA = 6;
    public const int CONSOLE = 7;
    public const int EOM = 8;
    public const int EXEC = 9;
    public const int LOG = 10;
    public const int NL = 11;
    public const int NOTIFY = 12;
    public const int RESULT = 13;
    public const int RESULT_CLASS = 14;
    public const int STATUS = 15;
    public const int STRING = 16;
    public const int TARGET = 17;
    public const int TOKEN = 18;
    public const int WS = 19;

    // delegates
    // delegators



        public GDBMIParser(ITokenStream input)
    		: this(input, new RecognizerSharedState()) {
        }

        public GDBMIParser(ITokenStream input, RecognizerSharedState state)
    		: base(input, state) {
            InitializeCyclicDFAs();

             
        }
        

    override public string[] TokenNames {
		get { return GDBMIParser.tokenNames; }
    }

    override public string GrammarFileName {
		get { return "GDBMI.g"; }
    }

    		
    	public class GDBMIRecord : Dictionary<string, object>
    	{			
    		public string token = null;
    							
    		public override string ToString()
    		{
    			string values = "(";
    			foreach(KeyValuePair<string, object> pair in this) 
    				values += "'" + pair.Key + "' = '" + pair.Value + "', ";   
    			values += ")";
    		
    			return this.GetType().ToString() + ": token = '" + token + "' values = " + values;
    		}
    	}
    	
    	public class GDBMIResultRecord : GDBMIRecord
    	{
    		public enum ResultClass {none, done, running, connected, error, exit};
    		public ResultClass cls = ResultClass.none;
    		
    		public void SetClsFromString(string strCls)
    		{
    			switch(strCls)
    			{
    				case "done": cls = GDBMIResultRecord.ResultClass.done; break;
    				case "running": cls = GDBMIResultRecord.ResultClass.running; break;
    				case "connected": cls = GDBMIResultRecord.ResultClass.connected; break;
    				case "error": cls = GDBMIResultRecord.ResultClass.error; break;
    				case "exit": cls = GDBMIResultRecord.ResultClass.exit; break;
    				default: break;
    			}
    		}
    		
    		public override string ToString() 
    		{		
    			return this.GetType().ToString() + ": cls = " + cls + " " + base.ToString();
    		}
    	}
    	
    	public class GDBMIAsyncResultRecord : GDBMIRecord
    	{
    		public enum GDBMIAsyncResultRecordType {none, exc, status, notify};		
    		public enum AsyncClass {none, stopped, thread_group_created, thread_created, breakpoint};
    		
    		public AsyncClass cls = AsyncClass.none;
    		public GDBMIAsyncResultRecordType type = GDBMIAsyncResultRecordType.none;
    		
    		public void SetClsFromString(string strCls)
    		{
    			switch(strCls)
    			{
    				case "stopped": cls = GDBMIAsyncResultRecord.AsyncClass.stopped; break;
    				case "thread_group_created": cls = GDBMIAsyncResultRecord.AsyncClass.thread_group_created; break;
    				case "thread_created": cls = GDBMIAsyncResultRecord.AsyncClass.thread_created; break;
    				case "breakpoint": cls = GDBMIAsyncResultRecord.AsyncClass.breakpoint; break;
    				default: break;
    			}
    		}	
    		
    		public override string ToString() 
    		{		
    			return this.GetType().ToString() + ": cls = " + cls + " " + base.ToString();
    		}
    	}
    	
    	public class GDBMIStreamRecord
    	{
    		public enum GDBMIStreamRecordType {none, console, target, log};	
    		public GDBMIStreamRecordType type = GDBMIStreamRecordType.none;
    		public string str = null;
    		
    		public override string ToString() 
    		{
    			return this.GetType().ToString() + ": " + type + ": " + str;	
    		}
    	}
    	
    	public class GDBMIResponse
    	{
    		public List<GDBMIStreamRecord> stream = new List<GDBMIStreamRecord>();					
    		public List<GDBMIAsyncResultRecord> async = new List<GDBMIAsyncResultRecord>();			
    		public GDBMIResultRecord result = null;
    		
    		public override string ToString() 
    		{
    			string str = this.GetType().ToString() + ":\n";																						
    			foreach(GDBMIStreamRecord streamRecord in stream)
    				str += "\t" + streamRecord + "\n";					
    			foreach(GDBMIAsyncResultRecord asyncRec in async)
    				str += asyncRec + "\n";				
    			str += result + "\n";
    			
    			return str;
    		}
    	}
    	
    	public class Util
    	{
    		public static string Unescape(string str)
    		{
    			return str
    				.Replace("\\\\", "\\")
    				.Replace("\\\"", "\"")
    				.Replace("\\r", "\r")
    				.Replace("\\n", "\n")
    				.Replace("\\t", "\t");
    		}
    	}	
    	



    // $ANTLR start "output"
    // GDBMI.g:213:0: output returns [GDBMIResponse response] : ( out_of_band_record )* ( result_record )? EOF ;
    public GDBMIResponse output() // throws RecognitionException [1]
    {   
        GDBMIResponse response = default(GDBMIResponse);

        GDBMIParser.out_of_band_record_return out_of_band_record1 = default(GDBMIParser.out_of_band_record_return);

        GDBMIResultRecord result_record2 = default(GDBMIResultRecord);



        	response =  new GDBMIResponse();	

        try 
    	{
            // GDBMI.g:219:2: ( ( out_of_band_record )* ( result_record )? EOF )
            // GDBMI.g:219:2: ( out_of_band_record )* ( result_record )? EOF
            {
            	// GDBMI.g:219:2: ( out_of_band_record )*
            	do 
            	{
            	    int alt1 = 2;
            	    alt1 = dfa1.Predict(input);
            	    switch (alt1) 
            		{
            			case 1 :
            			    // GDBMI.g:220:3: out_of_band_record
            			    {
            			    	PushFollow(FOLLOW_out_of_band_record_in_output152);
            			    	out_of_band_record1 = out_of_band_record();
            			    	state.followingStackPointer--;

            			    			
            			    				if(((out_of_band_record1 != null) ? out_of_band_record1.stream : default(GDBMIStreamRecord)) != null)			
            			    					response.stream.Add(((out_of_band_record1 != null) ? out_of_band_record1.stream : default(GDBMIStreamRecord)));				
            			    				if(((out_of_band_record1 != null) ? out_of_band_record1.async : default(GDBMIAsyncResultRecord)) != null)
            			    					response.async.Add(((out_of_band_record1 != null) ? out_of_band_record1.async : default(GDBMIAsyncResultRecord)));
            			    			

            			    }
            			    break;

            			default:
            			    goto loop1;
            	    }
            	} while (true);

            	loop1:
            		;	// Stops C# compiler whining that label 'loop1' has no statements

            	// GDBMI.g:229:2: ( result_record )?
            	int alt2 = 2;
            	int LA2_0 = input.LA(1);

            	if ( (LA2_0 == RESULT || LA2_0 == TOKEN) )
            	{
            	    alt2 = 1;
            	}
            	switch (alt2) 
            	{
            	    case 1 :
            	        // GDBMI.g:229:0: result_record
            	        {
            	        	PushFollow(FOLLOW_result_record_in_output167);
            	        	result_record2 = result_record();
            	        	state.followingStackPointer--;


            	        }
            	        break;

            	}

            	 
            			//Logger.Instance.Debug("result_record");		
            			response.result = result_record2;		
            		
            	Match(input,EOF,FOLLOW_EOF_in_output180); 

            }

        }
        catch (RecognitionException re) 
    	{
            ReportError(re);
            Recover(input,re);
        }
        finally 
    	{
        }
        return response;
    }
    // $ANTLR end "output"


    // $ANTLR start "result_record"
    // GDBMI.g:240:0: result_record returns [GDBMIResultRecord val] : ( TOKEN )? RESULT RESULT_CLASS ( COMMA result )* ( WS )* NL ;
    public GDBMIResultRecord result_record() // throws RecognitionException [1]
    {   
        GDBMIResultRecord val = default(GDBMIResultRecord);

        IToken TOKEN3 = null;
        IToken RESULT_CLASS4 = null;
        GDBMIParser.result_return result5 = default(GDBMIParser.result_return);



        	val =  new GDBMIResultRecord();

        try 
    	{
            // GDBMI.g:246:2: ( ( TOKEN )? RESULT RESULT_CLASS ( COMMA result )* ( WS )* NL )
            // GDBMI.g:246:2: ( TOKEN )? RESULT RESULT_CLASS ( COMMA result )* ( WS )* NL
            {
            	// GDBMI.g:246:2: ( TOKEN )?
            	int alt3 = 2;
            	int LA3_0 = input.LA(1);

            	if ( (LA3_0 == TOKEN) )
            	{
            	    alt3 = 1;
            	}
            	switch (alt3) 
            	{
            	    case 1 :
            	        // GDBMI.g:247:3: TOKEN
            	        {
            	        	TOKEN3=(IToken)Match(input,TOKEN,FOLLOW_TOKEN_in_result_record206); 
            	        	 val.token = ((TOKEN3 != null) ? TOKEN3.Text : null); 

            	        }
            	        break;

            	}

            	Match(input,RESULT,FOLLOW_RESULT_in_result_record217); 
            	RESULT_CLASS4=(IToken)Match(input,RESULT_CLASS,FOLLOW_RESULT_CLASS_in_result_record221); 
            	 
            			val.SetClsFromString(((RESULT_CLASS4 != null) ? RESULT_CLASS4.Text : null));
            		
            	// GDBMI.g:254:2: ( COMMA result )*
            	do 
            	{
            	    int alt4 = 2;
            	    int LA4_0 = input.LA(1);

            	    if ( (LA4_0 == COMMA) )
            	    {
            	        alt4 = 1;
            	    }


            	    switch (alt4) 
            		{
            			case 1 :
            			    // GDBMI.g:255:3: COMMA result
            			    {
            			    	Match(input,COMMA,FOLLOW_COMMA_in_result_record233); 
            			    	PushFollow(FOLLOW_result_in_result_record235);
            			    	result5 = result();
            			    	state.followingStackPointer--;

            			    	 val[((result5 != null) ? result5.key : default(string))] = ((result5 != null) ? result5.val : default(object)); 

            			    }
            			    break;

            			default:
            			    goto loop4;
            	    }
            	} while (true);

            	loop4:
            		;	// Stops C# compiler whining that label 'loop4' has no statements

            	// GDBMI.g:257:2: ( WS )*
            	do 
            	{
            	    int alt5 = 2;
            	    int LA5_0 = input.LA(1);

            	    if ( (LA5_0 == WS) )
            	    {
            	        alt5 = 1;
            	    }


            	    switch (alt5) 
            		{
            			case 1 :
            			    // GDBMI.g:257:0: WS
            			    {
            			    	Match(input,WS,FOLLOW_WS_in_result_record246); 

            			    }
            			    break;

            			default:
            			    goto loop5;
            	    }
            	} while (true);

            	loop5:
            		;	// Stops C# compiler whining that label 'loop5' has no statements

            	Match(input,NL,FOLLOW_NL_in_result_record251); 

            }

        }
        catch (RecognitionException re) 
    	{
            ReportError(re);
            Recover(input,re);
        }
        finally 
    	{
        }
        return val;
    }
    // $ANTLR end "result_record"

    public class out_of_band_record_return : ParserRuleReturnScope
    {
        public GDBMIAsyncResultRecord async;
        public GDBMIStreamRecord stream;
    };

    // $ANTLR start "out_of_band_record"
    // GDBMI.g:261:0: out_of_band_record returns [GDBMIAsyncResultRecord async, \r\n\t\t\t\t\t\t\tGDBMIStreamRecord stream] : ( async_record | stream_record );
    public GDBMIParser.out_of_band_record_return out_of_band_record() // throws RecognitionException [1]
    {   
        GDBMIParser.out_of_band_record_return retval = new GDBMIParser.out_of_band_record_return();
        retval.Start = input.LT(1);

        GDBMIAsyncResultRecord async_record6 = default(GDBMIAsyncResultRecord);

        GDBMIStreamRecord stream_record7 = default(GDBMIStreamRecord);



        	retval.stream =  null;
        	retval.async =  null;

        try 
    	{
            // GDBMI.g:269:2: ( async_record | stream_record )
            int alt6 = 2;
            int LA6_0 = input.LA(1);

            if ( (LA6_0 == EXEC || LA6_0 == NOTIFY || LA6_0 == STATUS || LA6_0 == TOKEN) )
            {
                alt6 = 1;
            }
            else if ( (LA6_0 == CONSOLE || LA6_0 == LOG || LA6_0 == TARGET) )
            {
                alt6 = 2;
            }
            else 
            {
                NoViableAltException nvae_d6s0 =
                    new NoViableAltException("", 6, 0, input);

                throw nvae_d6s0;
            }
            switch (alt6) 
            {
                case 1 :
                    // GDBMI.g:269:2: async_record
                    {
                    	PushFollow(FOLLOW_async_record_in_out_of_band_record272);
                    	async_record6 = async_record();
                    	state.followingStackPointer--;

                    	 
                    			retval.async =  async_record6; 
                    			//Logger.Instance.Debug("async_record");
                    		

                    }
                    break;
                case 2 :
                    // GDBMI.g:275:2: stream_record
                    {
                    	PushFollow(FOLLOW_stream_record_in_out_of_band_record284);
                    	stream_record7 = stream_record();
                    	state.followingStackPointer--;

                    	 
                    			retval.stream =  stream_record7; 
                    			//Logger.Instance.Debug("stream_record");
                    		

                    }
                    break;

            }
            retval.Stop = input.LT(-1);

        }
        catch (RecognitionException re) 
    	{
            ReportError(re);
            Recover(input,re);
        }
        finally 
    	{
        }
        return retval;
    }
    // $ANTLR end "out_of_band_record"


    // $ANTLR start "async_record"
    // GDBMI.g:282:0: async_record returns [GDBMIAsyncResultRecord val] : ( exec_async_output | status_async_output | notify_async_output );
    public GDBMIAsyncResultRecord async_record() // throws RecognitionException [1]
    {   
        GDBMIAsyncResultRecord val = default(GDBMIAsyncResultRecord);

        GDBMIAsyncResultRecord exec_async_output8 = default(GDBMIAsyncResultRecord);

        GDBMIAsyncResultRecord status_async_output9 = default(GDBMIAsyncResultRecord);

        GDBMIAsyncResultRecord notify_async_output10 = default(GDBMIAsyncResultRecord);


        try 
    	{
            // GDBMI.g:284:2: ( exec_async_output | status_async_output | notify_async_output )
            int alt7 = 3;
            switch ( input.LA(1) ) 
            {
            case TOKEN:
            	{
                switch ( input.LA(2) ) 
                {
                case EXEC:
                	{
                    alt7 = 1;
                    }
                    break;
                case STATUS:
                	{
                    alt7 = 2;
                    }
                    break;
                case NOTIFY:
                	{
                    alt7 = 3;
                    }
                    break;
                	default:
                	    NoViableAltException nvae_d7s1 =
                	        new NoViableAltException("", 7, 1, input);

                	    throw nvae_d7s1;
                }

                }
                break;
            case EXEC:
            	{
                alt7 = 1;
                }
                break;
            case STATUS:
            	{
                alt7 = 2;
                }
                break;
            case NOTIFY:
            	{
                alt7 = 3;
                }
                break;
            	default:
            	    NoViableAltException nvae_d7s0 =
            	        new NoViableAltException("", 7, 0, input);

            	    throw nvae_d7s0;
            }

            switch (alt7) 
            {
                case 1 :
                    // GDBMI.g:284:2: exec_async_output
                    {
                    	PushFollow(FOLLOW_exec_async_output_in_async_record302);
                    	exec_async_output8 = exec_async_output();
                    	state.followingStackPointer--;


                    			val =  exec_async_output8;
                    			val.type = GDBMIAsyncResultRecord.GDBMIAsyncResultRecordType.exc;
                    		

                    }
                    break;
                case 2 :
                    // GDBMI.g:290:2: status_async_output
                    {
                    	PushFollow(FOLLOW_status_async_output_in_async_record314);
                    	status_async_output9 = status_async_output();
                    	state.followingStackPointer--;


                    			val =  status_async_output9;
                    			val.type = GDBMIAsyncResultRecord.GDBMIAsyncResultRecordType.status;
                    		

                    }
                    break;
                case 3 :
                    // GDBMI.g:296:2: notify_async_output
                    {
                    	PushFollow(FOLLOW_notify_async_output_in_async_record326);
                    	notify_async_output10 = notify_async_output();
                    	state.followingStackPointer--;


                    			val =  notify_async_output10;
                    			val.type = GDBMIAsyncResultRecord.GDBMIAsyncResultRecordType.notify;
                    		

                    }
                    break;

            }
        }
        catch (RecognitionException re) 
    	{
            ReportError(re);
            Recover(input,re);
        }
        finally 
    	{
        }
        return val;
    }
    // $ANTLR end "async_record"


    // $ANTLR start "exec_async_output"
    // GDBMI.g:303:0: exec_async_output returns [GDBMIAsyncResultRecord val] : ( TOKEN )? EXEC async_output ;
    public GDBMIAsyncResultRecord exec_async_output() // throws RecognitionException [1]
    {   
        GDBMIAsyncResultRecord val = default(GDBMIAsyncResultRecord);

        GDBMIAsyncResultRecord async_output11 = default(GDBMIAsyncResultRecord);


        try 
    	{
            // GDBMI.g:305:2: ( ( TOKEN )? EXEC async_output )
            // GDBMI.g:305:2: ( TOKEN )? EXEC async_output
            {
            	// GDBMI.g:305:2: ( TOKEN )?
            	int alt8 = 2;
            	int LA8_0 = input.LA(1);

            	if ( (LA8_0 == TOKEN) )
            	{
            	    alt8 = 1;
            	}
            	switch (alt8) 
            	{
            	    case 1 :
            	        // GDBMI.g:305:3: TOKEN
            	        {
            	        	Match(input,TOKEN,FOLLOW_TOKEN_in_exec_async_output346); 

            	        }
            	        break;

            	}

            	Match(input,EXEC,FOLLOW_EXEC_in_exec_async_output350); 
            	PushFollow(FOLLOW_async_output_in_exec_async_output352);
            	async_output11 = async_output();
            	state.followingStackPointer--;

            	 val =  async_output11; 

            }

        }
        catch (RecognitionException re) 
    	{
            ReportError(re);
            Recover(input,re);
        }
        finally 
    	{
        }
        return val;
    }
    // $ANTLR end "exec_async_output"


    // $ANTLR start "status_async_output"
    // GDBMI.g:308:0: status_async_output returns [GDBMIAsyncResultRecord val] : ( TOKEN )? STATUS async_output ;
    public GDBMIAsyncResultRecord status_async_output() // throws RecognitionException [1]
    {   
        GDBMIAsyncResultRecord val = default(GDBMIAsyncResultRecord);

        GDBMIAsyncResultRecord async_output12 = default(GDBMIAsyncResultRecord);


        try 
    	{
            // GDBMI.g:310:2: ( ( TOKEN )? STATUS async_output )
            // GDBMI.g:310:2: ( TOKEN )? STATUS async_output
            {
            	// GDBMI.g:310:2: ( TOKEN )?
            	int alt9 = 2;
            	int LA9_0 = input.LA(1);

            	if ( (LA9_0 == TOKEN) )
            	{
            	    alt9 = 1;
            	}
            	switch (alt9) 
            	{
            	    case 1 :
            	        // GDBMI.g:310:3: TOKEN
            	        {
            	        	Match(input,TOKEN,FOLLOW_TOKEN_in_status_async_output371); 

            	        }
            	        break;

            	}

            	Match(input,STATUS,FOLLOW_STATUS_in_status_async_output375); 
            	PushFollow(FOLLOW_async_output_in_status_async_output377);
            	async_output12 = async_output();
            	state.followingStackPointer--;

            	 val =  async_output12; 

            }

        }
        catch (RecognitionException re) 
    	{
            ReportError(re);
            Recover(input,re);
        }
        finally 
    	{
        }
        return val;
    }
    // $ANTLR end "status_async_output"


    // $ANTLR start "notify_async_output"
    // GDBMI.g:313:0: notify_async_output returns [GDBMIAsyncResultRecord val] : ( TOKEN )? NOTIFY async_output ;
    public GDBMIAsyncResultRecord notify_async_output() // throws RecognitionException [1]
    {   
        GDBMIAsyncResultRecord val = default(GDBMIAsyncResultRecord);

        GDBMIAsyncResultRecord async_output13 = default(GDBMIAsyncResultRecord);


        try 
    	{
            // GDBMI.g:315:2: ( ( TOKEN )? NOTIFY async_output )
            // GDBMI.g:315:2: ( TOKEN )? NOTIFY async_output
            {
            	// GDBMI.g:315:2: ( TOKEN )?
            	int alt10 = 2;
            	int LA10_0 = input.LA(1);

            	if ( (LA10_0 == TOKEN) )
            	{
            	    alt10 = 1;
            	}
            	switch (alt10) 
            	{
            	    case 1 :
            	        // GDBMI.g:315:3: TOKEN
            	        {
            	        	Match(input,TOKEN,FOLLOW_TOKEN_in_notify_async_output396); 

            	        }
            	        break;

            	}

            	Match(input,NOTIFY,FOLLOW_NOTIFY_in_notify_async_output400); 
            	PushFollow(FOLLOW_async_output_in_notify_async_output402);
            	async_output13 = async_output();
            	state.followingStackPointer--;

            	val =  async_output13;

            }

        }
        catch (RecognitionException re) 
    	{
            ReportError(re);
            Recover(input,re);
        }
        finally 
    	{
        }
        return val;
    }
    // $ANTLR end "notify_async_output"


    // $ANTLR start "async_output"
    // GDBMI.g:318:0: async_output returns [GDBMIAsyncResultRecord val] : ASYNC_CLASS ( COMMA result )* NL ;
    public GDBMIAsyncResultRecord async_output() // throws RecognitionException [1]
    {   
        GDBMIAsyncResultRecord val = default(GDBMIAsyncResultRecord);

        IToken ASYNC_CLASS14 = null;
        GDBMIParser.result_return result15 = default(GDBMIParser.result_return);



        	val =  new GDBMIAsyncResultRecord();

        try 
    	{
            // GDBMI.g:324:2: ( ASYNC_CLASS ( COMMA result )* NL )
            // GDBMI.g:324:2: ASYNC_CLASS ( COMMA result )* NL
            {
            	ASYNC_CLASS14=(IToken)Match(input,ASYNC_CLASS,FOLLOW_ASYNC_CLASS_in_async_output426); 
            		
            			val.SetClsFromString(((ASYNC_CLASS14 != null) ? ASYNC_CLASS14.Text : null));
            		
            	// GDBMI.g:328:2: ( COMMA result )*
            	do 
            	{
            	    int alt11 = 2;
            	    int LA11_0 = input.LA(1);

            	    if ( (LA11_0 == COMMA) )
            	    {
            	        alt11 = 1;
            	    }


            	    switch (alt11) 
            		{
            			case 1 :
            			    // GDBMI.g:329:3: COMMA result
            			    {
            			    	Match(input,COMMA,FOLLOW_COMMA_in_async_output438); 
            			    	PushFollow(FOLLOW_result_in_async_output443);
            			    	result15 = result();
            			    	state.followingStackPointer--;

            			    	 
            			    				val[((result15 != null) ? result15.key : default(string))] = ((result15 != null) ? result15.val : default(object)); 
            			    			

            			    }
            			    break;

            			default:
            			    goto loop11;
            	    }
            	} while (true);

            	loop11:
            		;	// Stops C# compiler whining that label 'loop11' has no statements

            	Match(input,NL,FOLLOW_NL_in_async_output456); 

            }

        }
        catch (RecognitionException re) 
    	{
            ReportError(re);
            Recover(input,re);
        }
        finally 
    	{
        }
        return val;
    }
    // $ANTLR end "async_output"


    // $ANTLR start "var"
    // GDBMI.g:338:0: var returns [string txt] : STRING ;
    public string var() // throws RecognitionException [1]
    {   
        string txt = default(string);

        IToken STRING16 = null;

        try 
    	{
            // GDBMI.g:340:2: ( STRING )
            // GDBMI.g:340:2: STRING
            {
            	STRING16=(IToken)Match(input,STRING,FOLLOW_STRING_in_var473); 

            			txt =  ((STRING16 != null) ? STRING16.Text : null);
            		

            }

        }
        catch (RecognitionException re) 
    	{
            ReportError(re);
            Recover(input,re);
        }
        finally 
    	{
        }
        return txt;
    }
    // $ANTLR end "var"


    // $ANTLR start "value"
    // GDBMI.g:346:0: value returns [object val] : ( const_ | tuple | list );
    public object value() // throws RecognitionException [1]
    {   
        object val = default(object);

        string const_17 = default(string);

        List<object> tuple18 = default(List<object>);

        List<object> list19 = default(List<object>);


        try 
    	{
            // GDBMI.g:348:2: ( const_ | tuple | list )
            int alt12 = 3;
            switch ( input.LA(1) ) 
            {
            case C_STRING:
            	{
                alt12 = 1;
                }
                break;
            case 23:
            case 24:
            	{
                alt12 = 2;
                }
                break;
            case 20:
            case 21:
            	{
                alt12 = 3;
                }
                break;
            	default:
            	    NoViableAltException nvae_d12s0 =
            	        new NoViableAltException("", 12, 0, input);

            	    throw nvae_d12s0;
            }

            switch (alt12) 
            {
                case 1 :
                    // GDBMI.g:348:2: const_
                    {
                    	PushFollow(FOLLOW_const__in_value494);
                    	const_17 = const_();
                    	state.followingStackPointer--;

                    	val =  const_17;

                    }
                    break;
                case 2 :
                    // GDBMI.g:350:2: tuple
                    {
                    	PushFollow(FOLLOW_tuple_in_value504);
                    	tuple18 = tuple();
                    	state.followingStackPointer--;

                    	val =  tuple18;

                    }
                    break;
                case 3 :
                    // GDBMI.g:352:2: list
                    {
                    	PushFollow(FOLLOW_list_in_value514);
                    	list19 = list();
                    	state.followingStackPointer--;

                    	val =  list19;

                    }
                    break;

            }
        }
        catch (RecognitionException re) 
    	{
            ReportError(re);
            Recover(input,re);
        }
        finally 
    	{
        }
        return val;
    }
    // $ANTLR end "value"


    // $ANTLR start "const_"
    // GDBMI.g:355:0: const_ returns [string txt] : C_STRING ;
    public string const_() // throws RecognitionException [1]
    {   
        string txt = default(string);

        IToken C_STRING20 = null;

        try 
    	{
            // GDBMI.g:357:2: ( C_STRING )
            // GDBMI.g:357:2: C_STRING
            {
            	C_STRING20=(IToken)Match(input,C_STRING,FOLLOW_C_STRING_in_const_532); 
            	 txt =  ((C_STRING20 != null) ? C_STRING20.Text : null); 

            }

        }
        catch (RecognitionException re) 
    	{
            ReportError(re);
            Recover(input,re);
        }
        finally 
    	{
        }
        return txt;
    }
    // $ANTLR end "const_"

    public class result_return : ParserRuleReturnScope
    {
        public string key;
        public object val;
    };

    // $ANTLR start "result"
    // GDBMI.g:360:0: result returns [string key, object val] : ( var '=' value ) ;
    public GDBMIParser.result_return result() // throws RecognitionException [1]
    {   
        GDBMIParser.result_return retval = new GDBMIParser.result_return();
        retval.Start = input.LT(1);

        string var21 = default(string);

        object value22 = default(object);


        try 
    	{
            // GDBMI.g:362:2: ( ( var '=' value ) )
            // GDBMI.g:362:2: ( var '=' value )
            {
            	// GDBMI.g:362:2: ( var '=' value )
            	// GDBMI.g:362:3: var '=' value
            	{
            		PushFollow(FOLLOW_var_in_result551);
            		var21 = var();
            		state.followingStackPointer--;

            		Match(input,NOTIFY,FOLLOW_NOTIFY_in_result553); 
            		PushFollow(FOLLOW_value_in_result555);
            		value22 = value();
            		state.followingStackPointer--;


            	}


            			retval.key =  var21;
            			retval.val =  value22;
            		

            }

            retval.Stop = input.LT(-1);

        }
        catch (RecognitionException re) 
    	{
            ReportError(re);
            Recover(input,re);
        }
        finally 
    	{
        }
        return retval;
    }
    // $ANTLR end "result"


    // $ANTLR start "stream_record"
    // GDBMI.g:369:0: stream_record returns [GDBMIStreamRecord val] : ( console_stream_output | target_stream_output | log_stream_output ) NL ;
    public GDBMIStreamRecord stream_record() // throws RecognitionException [1]
    {   
        GDBMIStreamRecord val = default(GDBMIStreamRecord);

        string console_stream_output23 = default(string);

        string target_stream_output24 = default(string);

        string log_stream_output25 = default(string);



        	val =  new GDBMIStreamRecord();

        try 
    	{
            // GDBMI.g:375:2: ( ( console_stream_output | target_stream_output | log_stream_output ) NL )
            // GDBMI.g:375:2: ( console_stream_output | target_stream_output | log_stream_output ) NL
            {
            	// GDBMI.g:375:2: ( console_stream_output | target_stream_output | log_stream_output )
            	int alt13 = 3;
            	switch ( input.LA(1) ) 
            	{
            	case CONSOLE:
            		{
            	    alt13 = 1;
            	    }
            	    break;
            	case TARGET:
            		{
            	    alt13 = 2;
            	    }
            	    break;
            	case LOG:
            		{
            	    alt13 = 3;
            	    }
            	    break;
            		default:
            		    NoViableAltException nvae_d13s0 =
            		        new NoViableAltException("", 13, 0, input);

            		    throw nvae_d13s0;
            	}

            	switch (alt13) 
            	{
            	    case 1 :
            	        // GDBMI.g:376:3: console_stream_output
            	        {
            	        	PushFollow(FOLLOW_console_stream_output_in_stream_record586);
            	        	console_stream_output23 = console_stream_output();
            	        	state.followingStackPointer--;


            	        				val.str = console_stream_output23;
            	        				val.type = GDBMIStreamRecord.GDBMIStreamRecordType.console;
            	        			

            	        }
            	        break;
            	    case 2 :
            	        // GDBMI.g:382:3: target_stream_output
            	        {
            	        	PushFollow(FOLLOW_target_stream_output_in_stream_record600);
            	        	target_stream_output24 = target_stream_output();
            	        	state.followingStackPointer--;


            	        				val.str = target_stream_output24;
            	        				val.type = GDBMIStreamRecord.GDBMIStreamRecordType.target;
            	        			

            	        }
            	        break;
            	    case 3 :
            	        // GDBMI.g:388:3: log_stream_output
            	        {
            	        	PushFollow(FOLLOW_log_stream_output_in_stream_record614);
            	        	log_stream_output25 = log_stream_output();
            	        	state.followingStackPointer--;


            	        				val.str = log_stream_output25;
            	        				val.type = GDBMIStreamRecord.GDBMIStreamRecordType.log;
            	        			

            	        }
            	        break;

            	}

            	Match(input,NL,FOLLOW_NL_in_stream_record625); 

            }

        }
        catch (RecognitionException re) 
    	{
            ReportError(re);
            Recover(input,re);
        }
        finally 
    	{
        }
        return val;
    }
    // $ANTLR end "stream_record"


    // $ANTLR start "tuple"
    // GDBMI.g:397:0: tuple returns [List<object> items] : ( '{}' | '{' a= result ( COMMA b= result )* '}' );
    public List<object> tuple() // throws RecognitionException [1]
    {   
        List<object> items = default(List<object>);

        GDBMIParser.result_return a = default(GDBMIParser.result_return);

        GDBMIParser.result_return b = default(GDBMIParser.result_return);



        	items =  new List<object>(); 

        try 
    	{
            // GDBMI.g:403:2: ( '{}' | '{' a= result ( COMMA b= result )* '}' )
            int alt15 = 2;
            int LA15_0 = input.LA(1);

            if ( (LA15_0 == 24) )
            {
                alt15 = 1;
            }
            else if ( (LA15_0 == 23) )
            {
                alt15 = 2;
            }
            else 
            {
                NoViableAltException nvae_d15s0 =
                    new NoViableAltException("", 15, 0, input);

                throw nvae_d15s0;
            }
            switch (alt15) 
            {
                case 1 :
                    // GDBMI.g:403:2: '{}'
                    {
                    	Match(input,24,FOLLOW_24_in_tuple646); 

                    }
                    break;
                case 2 :
                    // GDBMI.g:405:2: '{' a= result ( COMMA b= result )* '}'
                    {
                    	Match(input,23,FOLLOW_23_in_tuple654); 

                    			Dictionary<string, object> dic = new Dictionary<string, object>();
                    		
                    	PushFollow(FOLLOW_result_in_tuple665);
                    	a = result();
                    	state.followingStackPointer--;

                    	 
                    			dic[a.key] = a.val;		
                    		
                    	// GDBMI.g:413:2: ( COMMA b= result )*
                    	do 
                    	{
                    	    int alt14 = 2;
                    	    int LA14_0 = input.LA(1);

                    	    if ( (LA14_0 == COMMA) )
                    	    {
                    	        alt14 = 1;
                    	    }


                    	    switch (alt14) 
                    		{
                    			case 1 :
                    			    // GDBMI.g:413:3: COMMA b= result
                    			    {
                    			    	Match(input,COMMA,FOLLOW_COMMA_in_tuple673); 
                    			    	PushFollow(FOLLOW_result_in_tuple681);
                    			    	b = result();
                    			    	state.followingStackPointer--;

                    			    	 
                    			    			dic[b.key] = b.val;	
                    			    		

                    			    }
                    			    break;

                    			default:
                    			    goto loop14;
                    	    }
                    	} while (true);

                    	loop14:
                    		;	// Stops C# compiler whining that label 'loop14' has no statements

                    	Match(input,25,FOLLOW_25_in_tuple693); 

                    			items.Add(dic);
                    		

                    }
                    break;

            }
        }
        catch (RecognitionException re) 
    	{
            ReportError(re);
            Recover(input,re);
        }
        finally 
    	{
        }
        return items;
    }
    // $ANTLR end "tuple"


    // $ANTLR start "list"
    // GDBMI.g:425:0: list returns [List<object> items] : ( '[]' | '[' a= value ( COMMA b= value )* ']' | '[' c= result ( COMMA d= result )* ']' );
    public List<object> list() // throws RecognitionException [1]
    {   
        List<object> items = default(List<object>);

        object a = default(object);

        object b = default(object);

        GDBMIParser.result_return c = default(GDBMIParser.result_return);

        GDBMIParser.result_return d = default(GDBMIParser.result_return);


         
        	items =  new List<object>(); 

        try 
    	{
            // GDBMI.g:431:2: ( '[]' | '[' a= value ( COMMA b= value )* ']' | '[' c= result ( COMMA d= result )* ']' )
            int alt18 = 3;
            int LA18_0 = input.LA(1);

            if ( (LA18_0 == 21) )
            {
                alt18 = 1;
            }
            else if ( (LA18_0 == 20) )
            {
                int LA18_2 = input.LA(2);

                if ( (LA18_2 == C_STRING || (LA18_2 >= 20 && LA18_2 <= 21) || (LA18_2 >= 23 && LA18_2 <= 24)) )
                {
                    alt18 = 2;
                }
                else if ( (LA18_2 == STRING) )
                {
                    alt18 = 3;
                }
                else 
                {
                    NoViableAltException nvae_d18s2 =
                        new NoViableAltException("", 18, 2, input);

                    throw nvae_d18s2;
                }
            }
            else 
            {
                NoViableAltException nvae_d18s0 =
                    new NoViableAltException("", 18, 0, input);

                throw nvae_d18s0;
            }
            switch (alt18) 
            {
                case 1 :
                    // GDBMI.g:431:2: '[]'
                    {
                    	Match(input,21,FOLLOW_21_in_list718); 

                    }
                    break;
                case 2 :
                    // GDBMI.g:433:2: '[' a= value ( COMMA b= value )* ']'
                    {
                    	Match(input,20,FOLLOW_20_in_list726); 
                    	PushFollow(FOLLOW_value_in_list732);
                    	a = value();
                    	state.followingStackPointer--;

                    	items.Add(a);
                    	// GDBMI.g:433:33: ( COMMA b= value )*
                    	do 
                    	{
                    	    int alt16 = 2;
                    	    int LA16_0 = input.LA(1);

                    	    if ( (LA16_0 == COMMA) )
                    	    {
                    	        alt16 = 1;
                    	    }


                    	    switch (alt16) 
                    		{
                    			case 1 :
                    			    // GDBMI.g:433:35: COMMA b= value
                    			    {
                    			    	Match(input,COMMA,FOLLOW_COMMA_in_list738); 
                    			    	PushFollow(FOLLOW_value_in_list744);
                    			    	b = value();
                    			    	state.followingStackPointer--;

                    			    	items.Add(b);

                    			    }
                    			    break;

                    			default:
                    			    goto loop16;
                    	    }
                    	} while (true);

                    	loop16:
                    		;	// Stops C# compiler whining that label 'loop16' has no statements

                    	Match(input,22,FOLLOW_22_in_list751); 

                    }
                    break;
                case 3 :
                    // GDBMI.g:435:2: '[' c= result ( COMMA d= result )* ']'
                    {
                    	Match(input,20,FOLLOW_20_in_list758); 

                    			Dictionary<string, object> dic = new Dictionary<string, object>();
                    		
                    	PushFollow(FOLLOW_result_in_list769);
                    	c = result();
                    	state.followingStackPointer--;

                    	 
                    			dic[c.key] = c.val;		
                    		
                    	// GDBMI.g:443:2: ( COMMA d= result )*
                    	do 
                    	{
                    	    int alt17 = 2;
                    	    int LA17_0 = input.LA(1);

                    	    if ( (LA17_0 == COMMA) )
                    	    {
                    	        alt17 = 1;
                    	    }


                    	    switch (alt17) 
                    		{
                    			case 1 :
                    			    // GDBMI.g:443:3: COMMA d= result
                    			    {
                    			    	Match(input,COMMA,FOLLOW_COMMA_in_list778); 
                    			    	PushFollow(FOLLOW_result_in_list784);
                    			    	d = result();
                    			    	state.followingStackPointer--;

                    			    	 
                    			    			dic[d.key] = d.val;
                    			    		

                    			    }
                    			    break;

                    			default:
                    			    goto loop17;
                    	    }
                    	} while (true);

                    	loop17:
                    		;	// Stops C# compiler whining that label 'loop17' has no statements

                    	Match(input,22,FOLLOW_22_in_list796); 

                    			items.Add(dic);
                    		

                    }
                    break;

            }
        }
        catch (RecognitionException re) 
    	{
            ReportError(re);
            Recover(input,re);
        }
        finally 
    	{
        }
        return items;
    }
    // $ANTLR end "list"


    // $ANTLR start "console_stream_output"
    // GDBMI.g:454:0: console_stream_output returns [string txt] : CONSOLE C_STRING ;
    public string console_stream_output() // throws RecognitionException [1]
    {   
        string txt = default(string);

        IToken C_STRING26 = null;

        try 
    	{
            // GDBMI.g:456:2: ( CONSOLE C_STRING )
            // GDBMI.g:456:2: CONSOLE C_STRING
            {
            	Match(input,CONSOLE,FOLLOW_CONSOLE_in_console_stream_output814); 
            	C_STRING26=(IToken)Match(input,C_STRING,FOLLOW_C_STRING_in_console_stream_output816); 

            			txt =  Util.Unescape(((C_STRING26 != null) ? C_STRING26.Text : null).Trim(new char[]{'\"'}))
            					.TrimEnd(new char[]{'\n'})
            					.TrimEnd(new char[]{'\r'});
            							
            		

            }

        }
        catch (RecognitionException re) 
    	{
            ReportError(re);
            Recover(input,re);
        }
        finally 
    	{
        }
        return txt;
    }
    // $ANTLR end "console_stream_output"


    // $ANTLR start "target_stream_output"
    // GDBMI.g:465:0: target_stream_output returns [string txt] : TARGET C_STRING ;
    public string target_stream_output() // throws RecognitionException [1]
    {   
        string txt = default(string);

        IToken C_STRING27 = null;

        try 
    	{
            // GDBMI.g:467:2: ( TARGET C_STRING )
            // GDBMI.g:467:2: TARGET C_STRING
            {
            	Match(input,TARGET,FOLLOW_TARGET_in_target_stream_output834); 
            	C_STRING27=(IToken)Match(input,C_STRING,FOLLOW_C_STRING_in_target_stream_output836); 

            			txt =  Util.Unescape(((C_STRING27 != null) ? C_STRING27.Text : null).Trim(new char[]{'\"'}))
            					.TrimEnd(new char[]{'\n'})
            					.TrimEnd(new char[]{'\r'});		
            		

            }

        }
        catch (RecognitionException re) 
    	{
            ReportError(re);
            Recover(input,re);
        }
        finally 
    	{
        }
        return txt;
    }
    // $ANTLR end "target_stream_output"


    // $ANTLR start "log_stream_output"
    // GDBMI.g:475:0: log_stream_output returns [string txt] : LOG C_STRING ;
    public string log_stream_output() // throws RecognitionException [1]
    {   
        string txt = default(string);

        IToken C_STRING28 = null;

        try 
    	{
            // GDBMI.g:477:2: ( LOG C_STRING )
            // GDBMI.g:477:2: LOG C_STRING
            {
            	Match(input,LOG,FOLLOW_LOG_in_log_stream_output854); 
            	C_STRING28=(IToken)Match(input,C_STRING,FOLLOW_C_STRING_in_log_stream_output856); 

            			txt =  Util.Unescape(((C_STRING28 != null) ? C_STRING28.Text : null).Trim(new char[]{'\"'}))
            					.TrimEnd(new char[]{'\n'})
            					.TrimEnd(new char[]{'\r'});
            		

            }

        }
        catch (RecognitionException re) 
    	{
            ReportError(re);
            Recover(input,re);
        }
        finally 
    	{
        }
        return txt;
    }
    // $ANTLR end "log_stream_output"

    // Delegated rules


   	protected DFA1 dfa1;
	private void InitializeCyclicDFAs()
	{
    	this.dfa1 = new DFA1(this);
	}

    const string DFA1_eotS =
        "\xE\xFFFF";
    const string DFA1_eofS =
        "\x1\x2\xD\xFFFF";
    const string DFA1_minS =
        "\x1\x7\x1\x9\xC\xFFFF";
    const string DFA1_maxS =
        "\x1\x12\x1\xF\xC\xFFFF";
    const string DFA1_acceptS =
        "\x2\xFFFF\x1\x2\x1\xFFFF\x1\x1\x9\xFFFF";
    const string DFA1_specialS =
        "\xE\xFFFF}>";
    static readonly string[] DFA1_transitionS = {
            "\x1\x4\x1\xFFFF\x2\x4\x1\xFFFF\x1\x4\x1\x2\x1\xFFFF\x1\x4\x1"+
            "\xFFFF\x1\x4\x1\x1",
            "\x1\x4\x2\xFFFF\x1\x4\x1\x2\x1\xFFFF\x1\x4",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            ""
    };

    static readonly short[] DFA1_eot = DFA.UnpackEncodedString(DFA1_eotS);
    static readonly short[] DFA1_eof = DFA.UnpackEncodedString(DFA1_eofS);
    static readonly char[] DFA1_min = DFA.UnpackEncodedStringToUnsignedChars(DFA1_minS);
    static readonly char[] DFA1_max = DFA.UnpackEncodedStringToUnsignedChars(DFA1_maxS);
    static readonly short[] DFA1_accept = DFA.UnpackEncodedString(DFA1_acceptS);
    static readonly short[] DFA1_special = DFA.UnpackEncodedString(DFA1_specialS);
    static readonly short[][] DFA1_transition = DFA.UnpackEncodedStringArray(DFA1_transitionS);

    protected class DFA1 : DFA
    {
        public DFA1(BaseRecognizer recognizer)
        {
            this.recognizer = recognizer;
            this.decisionNumber = 1;
            this.eot = DFA1_eot;
            this.eof = DFA1_eof;
            this.min = DFA1_min;
            this.max = DFA1_max;
            this.accept = DFA1_accept;
            this.special = DFA1_special;
            this.transition = DFA1_transition;

        }

        override public string Description
        {
            get { return "()* loopback of 219:2: ( out_of_band_record )*"; }
        }

    }

 

    public static readonly BitSet FOLLOW_out_of_band_record_in_output152 = new BitSet(new ulong[]{0x000000000006B680UL});
    public static readonly BitSet FOLLOW_result_record_in_output167 = new BitSet(new ulong[]{0x0000000000000000UL});
    public static readonly BitSet FOLLOW_EOF_in_output180 = new BitSet(new ulong[]{0x0000000000000002UL});
    public static readonly BitSet FOLLOW_TOKEN_in_result_record206 = new BitSet(new ulong[]{0x0000000000002000UL});
    public static readonly BitSet FOLLOW_RESULT_in_result_record217 = new BitSet(new ulong[]{0x0000000000004000UL});
    public static readonly BitSet FOLLOW_RESULT_CLASS_in_result_record221 = new BitSet(new ulong[]{0x0000000000080840UL});
    public static readonly BitSet FOLLOW_COMMA_in_result_record233 = new BitSet(new ulong[]{0x0000000000010000UL});
    public static readonly BitSet FOLLOW_result_in_result_record235 = new BitSet(new ulong[]{0x0000000000080840UL});
    public static readonly BitSet FOLLOW_WS_in_result_record246 = new BitSet(new ulong[]{0x0000000000080800UL});
    public static readonly BitSet FOLLOW_NL_in_result_record251 = new BitSet(new ulong[]{0x0000000000000002UL});
    public static readonly BitSet FOLLOW_async_record_in_out_of_band_record272 = new BitSet(new ulong[]{0x0000000000000002UL});
    public static readonly BitSet FOLLOW_stream_record_in_out_of_band_record284 = new BitSet(new ulong[]{0x0000000000000002UL});
    public static readonly BitSet FOLLOW_exec_async_output_in_async_record302 = new BitSet(new ulong[]{0x0000000000000002UL});
    public static readonly BitSet FOLLOW_status_async_output_in_async_record314 = new BitSet(new ulong[]{0x0000000000000002UL});
    public static readonly BitSet FOLLOW_notify_async_output_in_async_record326 = new BitSet(new ulong[]{0x0000000000000002UL});
    public static readonly BitSet FOLLOW_TOKEN_in_exec_async_output346 = new BitSet(new ulong[]{0x0000000000000200UL});
    public static readonly BitSet FOLLOW_EXEC_in_exec_async_output350 = new BitSet(new ulong[]{0x0000000000000010UL});
    public static readonly BitSet FOLLOW_async_output_in_exec_async_output352 = new BitSet(new ulong[]{0x0000000000000002UL});
    public static readonly BitSet FOLLOW_TOKEN_in_status_async_output371 = new BitSet(new ulong[]{0x0000000000008000UL});
    public static readonly BitSet FOLLOW_STATUS_in_status_async_output375 = new BitSet(new ulong[]{0x0000000000000010UL});
    public static readonly BitSet FOLLOW_async_output_in_status_async_output377 = new BitSet(new ulong[]{0x0000000000000002UL});
    public static readonly BitSet FOLLOW_TOKEN_in_notify_async_output396 = new BitSet(new ulong[]{0x0000000000001000UL});
    public static readonly BitSet FOLLOW_NOTIFY_in_notify_async_output400 = new BitSet(new ulong[]{0x0000000000000010UL});
    public static readonly BitSet FOLLOW_async_output_in_notify_async_output402 = new BitSet(new ulong[]{0x0000000000000002UL});
    public static readonly BitSet FOLLOW_ASYNC_CLASS_in_async_output426 = new BitSet(new ulong[]{0x0000000000000840UL});
    public static readonly BitSet FOLLOW_COMMA_in_async_output438 = new BitSet(new ulong[]{0x0000000000010000UL});
    public static readonly BitSet FOLLOW_result_in_async_output443 = new BitSet(new ulong[]{0x0000000000000840UL});
    public static readonly BitSet FOLLOW_NL_in_async_output456 = new BitSet(new ulong[]{0x0000000000000002UL});
    public static readonly BitSet FOLLOW_STRING_in_var473 = new BitSet(new ulong[]{0x0000000000000002UL});
    public static readonly BitSet FOLLOW_const__in_value494 = new BitSet(new ulong[]{0x0000000000000002UL});
    public static readonly BitSet FOLLOW_tuple_in_value504 = new BitSet(new ulong[]{0x0000000000000002UL});
    public static readonly BitSet FOLLOW_list_in_value514 = new BitSet(new ulong[]{0x0000000000000002UL});
    public static readonly BitSet FOLLOW_C_STRING_in_const_532 = new BitSet(new ulong[]{0x0000000000000002UL});
    public static readonly BitSet FOLLOW_var_in_result551 = new BitSet(new ulong[]{0x0000000000001000UL});
    public static readonly BitSet FOLLOW_NOTIFY_in_result553 = new BitSet(new ulong[]{0x0000000001B00020UL});
    public static readonly BitSet FOLLOW_value_in_result555 = new BitSet(new ulong[]{0x0000000000000002UL});
    public static readonly BitSet FOLLOW_console_stream_output_in_stream_record586 = new BitSet(new ulong[]{0x0000000000000800UL});
    public static readonly BitSet FOLLOW_target_stream_output_in_stream_record600 = new BitSet(new ulong[]{0x0000000000000800UL});
    public static readonly BitSet FOLLOW_log_stream_output_in_stream_record614 = new BitSet(new ulong[]{0x0000000000000800UL});
    public static readonly BitSet FOLLOW_NL_in_stream_record625 = new BitSet(new ulong[]{0x0000000000000002UL});
    public static readonly BitSet FOLLOW_24_in_tuple646 = new BitSet(new ulong[]{0x0000000000000002UL});
    public static readonly BitSet FOLLOW_23_in_tuple654 = new BitSet(new ulong[]{0x0000000000010000UL});
    public static readonly BitSet FOLLOW_result_in_tuple665 = new BitSet(new ulong[]{0x0000000002000040UL});
    public static readonly BitSet FOLLOW_COMMA_in_tuple673 = new BitSet(new ulong[]{0x0000000000010000UL});
    public static readonly BitSet FOLLOW_result_in_tuple681 = new BitSet(new ulong[]{0x0000000002000040UL});
    public static readonly BitSet FOLLOW_25_in_tuple693 = new BitSet(new ulong[]{0x0000000000000002UL});
    public static readonly BitSet FOLLOW_21_in_list718 = new BitSet(new ulong[]{0x0000000000000002UL});
    public static readonly BitSet FOLLOW_20_in_list726 = new BitSet(new ulong[]{0x0000000001B00020UL});
    public static readonly BitSet FOLLOW_value_in_list732 = new BitSet(new ulong[]{0x0000000000400040UL});
    public static readonly BitSet FOLLOW_COMMA_in_list738 = new BitSet(new ulong[]{0x0000000001B00020UL});
    public static readonly BitSet FOLLOW_value_in_list744 = new BitSet(new ulong[]{0x0000000000400040UL});
    public static readonly BitSet FOLLOW_22_in_list751 = new BitSet(new ulong[]{0x0000000000000002UL});
    public static readonly BitSet FOLLOW_20_in_list758 = new BitSet(new ulong[]{0x0000000000010000UL});
    public static readonly BitSet FOLLOW_result_in_list769 = new BitSet(new ulong[]{0x0000000000400040UL});
    public static readonly BitSet FOLLOW_COMMA_in_list778 = new BitSet(new ulong[]{0x0000000000010000UL});
    public static readonly BitSet FOLLOW_result_in_list784 = new BitSet(new ulong[]{0x0000000000400040UL});
    public static readonly BitSet FOLLOW_22_in_list796 = new BitSet(new ulong[]{0x0000000000000002UL});
    public static readonly BitSet FOLLOW_CONSOLE_in_console_stream_output814 = new BitSet(new ulong[]{0x0000000000000020UL});
    public static readonly BitSet FOLLOW_C_STRING_in_console_stream_output816 = new BitSet(new ulong[]{0x0000000000000002UL});
    public static readonly BitSet FOLLOW_TARGET_in_target_stream_output834 = new BitSet(new ulong[]{0x0000000000000020UL});
    public static readonly BitSet FOLLOW_C_STRING_in_target_stream_output836 = new BitSet(new ulong[]{0x0000000000000002UL});
    public static readonly BitSet FOLLOW_LOG_in_log_stream_output854 = new BitSet(new ulong[]{0x0000000000000020UL});
    public static readonly BitSet FOLLOW_C_STRING_in_log_stream_output856 = new BitSet(new ulong[]{0x0000000000000002UL});

}
}