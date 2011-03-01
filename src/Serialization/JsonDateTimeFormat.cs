using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ManagedFusion.Serialization
{
	public enum JsonDateTimeFormat
	{
		Unix,
		RFC1123,
		ISO8601,

		/// <seelaos href="http://tools.ietf.org/html/draft-zyp-json-schema-02"/>
		UtcMillisec = Unix,

		/// <seelaos href="http://tools.ietf.org/html/draft-zyp-json-schema-02"/>
		DateTime = ISO8601,

		/// <seelaos href="http://tools.ietf.org/html/draft-zyp-json-schema-02"/>
		Date,

		/// <seelaos href="http://tools.ietf.org/html/draft-zyp-json-schema-02"/>
		Time
	}
}
