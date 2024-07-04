using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScnToolByAeven.SeqFile
{
	public enum Type { Particle, Particle_IF, NodeLoader, NodeLoader_IF, LerpNodeControl, NodeControl, Lightning, GhostTrail }
	class SeqFormat
	{
		public List<SeqNode> nodes;

		public static SeqFormat Load(string path)
		{
			return null;
		}
		public void Save(string path)
		{

		}
	}
}
