//
// PAME Copyright (c) George Samartzidis <samartzidis@gmail.com>. All rights reserved.
// You are not allowed to redistribute, modify or sell any part of this file in either 
// compiled or non-compiled form without the author's written permission.
//

//
// http://ftp.gnu.org/old-gnu/Manuals/gdb-5.1.1/html_node/gdb_214.html#SEC221
//
// The output from GDB/MI consists of zero or more out-of-band records followed, optionally, 
// by a single result record. This result record is for the most recent command. 
// The sequence of output records is terminated by '(gdb)'.
//
// If an input command was prefixed with a token then the corresponding output for that command 
// will also be prefixed by that same token.
//
//output ==>
//    ( out-of-band-record )* [ result-record ] "(gdb)" nl 
//result-record ==>
//    [ token ] "^" result-class ( "," result )* nl 
//out-of-band-record ==>
//    async-record | stream-record 
//async-record ==>
//    exec-async-output | status-async-output | notify-async-output 
//exec-async-output ==>
//    [ token ] "*" async-output 
//status-async-output ==>
//    [ token ] "+" async-output 
//notify-async-output ==>
//    [ token ] "=" async-output 
//async-output ==>
//    async-class ( "," result )* nl 
//result-class ==>
//    "done" | "running" | "connected" | "error" | "exit" 
//async-class ==>
//    "stopped" | others (where others will be added depending on the needs--this is still in development). 
//result ==>
//    variable "=" value 
//variable ==>
//    string 
//value ==>
//    const | tuple | list 
//const ==>
//    c-string 
//tuple ==>
//    "{}" | "{" result ( "," result )* "}" 
//list ==>
//    "[]" | "[" value ( "," value )* "]" | "[" result ( "," result )* "]" 
//stream-record ==>
//    console-stream-output | target-stream-output | log-stream-output 
//console-stream-output ==>
//    "~" c-string 
//target-stream-output ==>
//    "@" c-string 
//log-stream-output ==>
//    "&" c-string 
//nl ==>
//    CR | CR-LF 
//token ==>
//    any sequence of digits. 
//
// Notes:
// ------
// o All output sequences end in a single line containing a period.
// o The token is from the corresponding request. If an execution command is interrupted by 
//   the `-exec-interrupt' command, the token associated with the `*stopped' message is the one of 
//   the original execution command, not the one of the interrupt command.
// o status-async-output contains on-going status information about the progress of a slow operation. 
//   It can be discarded. All status output is prefixed by `+'.
//   exec-async-output contains asynchronous state change on the target (stopped, started, disappeared). 
// o All async output is prefixed by `*'.
// o notify-async-output contains supplementary information that the client should 
//   handle (e.g., a new breakpoint information). All notify output is prefixed by `='.
// o console-stream-output is output that should be displayed as is in the console. It is the textual 
//   response to a CLI command. All the console output is prefixed by `~'.
// o target-stream-output is the output produced by the target program. All the target output is prefixed by `@'.
// o log-stream-output is output text coming from GDB's internals, for instance messages that should be displayed 
//   as part of an error log. All the log output is prefixed by `&'.
// o New GDB/MI commands should only output lists containing values. 
//

grammar GDBMI;

options 
{
	language = CSharp2;
	k = 2;	
}

@namespace { GdbAdapter.Parsers }

@header
{		
	using System.Collections.Generic;
	//using Logging;
} //@header
 
@members
{		
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
	
} //@members

//PARSER

output returns [GDBMIResponse response]
@init 
{
	$response = new GDBMIResponse();	
}
: 
	(
		out_of_band_record 
		//NL
		{		
			if($out_of_band_record.stream != null)			
				$response.stream.Add($out_of_band_record.stream);				
			if($out_of_band_record.async != null)
				$response.async.Add($out_of_band_record.async);
		}
	)*
	result_record?
	{ 
		//Logger.Instance.Debug("result_record");		
		$response.result = $result_record.val;		
	}
	//EOM
	//WS* 
	//NL?	
	EOF
; 

result_record returns [GDBMIResultRecord val]
@init 
{
	$val = new GDBMIResultRecord();
}
: 
	(
		TOKEN { $val.token = $TOKEN.text; } 
	)? 
	RESULT 
	RESULT_CLASS 
	{ 
		$val.SetClsFromString($RESULT_CLASS.text);
	} 
	(
		COMMA result { $val[$result.key] = $result.val; } 
	)* 
	WS* 
	NL
;

out_of_band_record returns [GDBMIAsyncResultRecord async, 
							GDBMIStreamRecord stream]
@init 
{
	$stream = null;
	$async = null;
}
: 
	async_record 
	{ 
		$async = $async_record.val; 
		//Logger.Instance.Debug("async_record");
	} 
	| 
	stream_record 
	{ 
		$stream = $stream_record.val; 
		//Logger.Instance.Debug("stream_record");
	}
;

async_record returns [GDBMIAsyncResultRecord val]
:
	exec_async_output 
	{
		$val = $exec_async_output.val;
		$val.type = GDBMIAsyncResultRecord.GDBMIAsyncResultRecordType.exc;
	} 
	| 
	status_async_output 
	{
		$val = $status_async_output.val;
		$val.type = GDBMIAsyncResultRecord.GDBMIAsyncResultRecordType.status;
	} 
	| 
	notify_async_output 
	{
		$val = $notify_async_output.val;
		$val.type = GDBMIAsyncResultRecord.GDBMIAsyncResultRecordType.notify;
	}
;

exec_async_output returns [GDBMIAsyncResultRecord val]
: 
	(TOKEN)? EXEC async_output { $val = $async_output.val; }
;
	
status_async_output returns [GDBMIAsyncResultRecord val]
: 
	(TOKEN)? STATUS async_output { $val = $async_output.val; }
;
	
notify_async_output returns [GDBMIAsyncResultRecord val]
: 
	(TOKEN)? NOTIFY async_output {$val = $async_output.val;}
;
	
async_output returns [GDBMIAsyncResultRecord val]
@init 
{
	$val = new GDBMIAsyncResultRecord();
}
: 
	ASYNC_CLASS 
	{	
		$val.SetClsFromString($ASYNC_CLASS.text);
	} 
	(
		COMMA 
		result 
		{ 
			$val[$result.key] = $result.val; 
		}
	)* 
	NL
;
	
var	returns [string txt] 
: 
	STRING 
	{
		$txt = $STRING.text;
	}
;
	
value returns [object val] 
: 
	const_ {$val = $const_.txt;} 
	| 
	tuple {$val = $tuple.items;} 
	| 
	list {$val = $list.items;}
;
	
const_	returns [string txt]
: 
	C_STRING { $txt = $C_STRING.text; }
;
	
result returns [string key, object val]
: 
	(var '=' value) 
	{
		$key = $var.txt;
		$val = $value.val;
	}
;
	
stream_record returns [GDBMIStreamRecord val]
@init 
{
	$val = new GDBMIStreamRecord();
}
: 
	(
		console_stream_output 
		{
			$val.str = $console_stream_output.txt;
			$val.type = GDBMIStreamRecord.GDBMIStreamRecordType.console;
		}
		| 
		target_stream_output 
		{
			$val.str = $target_stream_output.txt;
			$val.type = GDBMIStreamRecord.GDBMIStreamRecordType.target;
		}
		| 
		log_stream_output 
		{
			$val.str = $log_stream_output.txt;
			$val.type = GDBMIStreamRecord.GDBMIStreamRecordType.log;
		}
	)
	NL
;
	
tuple returns [List<object> items]
@init
{
	$items = new List<object>(); 
}
: 
	'{}' 
	| 
	'{' 
	{
		Dictionary<string, object> dic = new Dictionary<string, object>();
	}
	a = result 
	{ 
		dic[a.key] = a.val;		
	}
	(COMMA 
	b = result 
	{ 
		dic[b.key] = b.val;	
	}
	)* 
	'}'
	{
		$items.Add(dic);
	}
;
	
list returns [List<object> items]
@init 
{ 
	$items = new List<object>(); 
}
: 
	'[]' 
	| 
	'[' a = value {$items.Add(a);} ( COMMA b = value {$items.Add(b);} )* ']'
	| 
	'[' 
	{
		Dictionary<string, object> dic = new Dictionary<string, object>();
	}
	c = result 
	{ 
		dic[c.key] = c.val;		
	} 
	(COMMA d = result 
	{ 
		dic[d.key] = d.val;
	}
	)* 
	']'
	{
		$items.Add(dic);
	}
;

console_stream_output returns [string txt]
: 
	CONSOLE C_STRING
	{
		$txt = Util.Unescape($C_STRING.text.Trim(new char[]{'\"'}))
				.TrimEnd(new char[]{'\n'})
				.TrimEnd(new char[]{'\r'});
						
	}
;

target_stream_output returns [string txt]
: 
	TARGET C_STRING
	{
		$txt = Util.Unescape($C_STRING.text.Trim(new char[]{'\"'}))
				.TrimEnd(new char[]{'\n'})
				.TrimEnd(new char[]{'\r'});		
	}
;

log_stream_output returns [string txt]
: 
	LOG C_STRING 
	{
		$txt = Util.Unescape($C_STRING.text.Trim(new char[]{'\"'}))
				.TrimEnd(new char[]{'\n'})
				.TrimEnd(new char[]{'\r'});
	}
;

// LEXER
	
C_STRING		
	: '"' ('\\''"' | ~('"' |'\n'|'\r'))* '"';

ASYNC_CLASS
	: 'stopped' | 'thread-group-created' | 'thread-created' | 'breakpoint';

RESULT_CLASS
	: 'done'
	| 'running'
	| 'connected'
	| 'error'
	| 'exit';

STRING
	: ('_' | 'A'..'Z' | 'a'..'z')('-' | '_' | 'A'..'Z' | 'a'..'z'|'0'..'9')*;

NL			
	: ('\r')?'\n';

WS
	: (' ' | '\t');
	
TOKEN 			
	: ('0'..'9')+;

COMMA	: ',';

EOM	: '(gdb)';	
	
CONSOLE : '~';
TARGET 	: '@';
LOG 	: '&';

EXEC 	: '*';
STATUS  : '+';
NOTIFY  : '=';

RESULT	: '^';



