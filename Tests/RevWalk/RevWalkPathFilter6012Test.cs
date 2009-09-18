/*
 * Copyright (C) 2009, Google Inc.
 *
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or
 * without modification, are permitted provided that the following
 * conditions are met:
 *
 * - Redistributions of source code must retain the above copyright
 *   notice, this list of conditions and the following disclaimer.
 *
 * - Redistributions in binary form must reproduce the above
 *   copyright notice, this list of conditions and the following
 *   disclaimer in the documentation and/or other materials provided
 *   with the distribution.
 *
 * - Neither the name of the Git Development Community nor the
 *   names of its contributors may be used to endorse or promote
 *   products derived from this software without specific prior
 *   written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND
 * CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
 * INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
 * CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
 * STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using GitSharp.RevWalk;
using GitSharp.TreeWalk.Filter;
using Xunit;

namespace GitSharp.Tests.RevWalk
{
	// Note: Much of this test case is broken as it depends upon
	// the graph applying topological sorting *before* doing merge
	// simplification.  It also depends upon a difference between
	// full history and non-full history for a path, something we
	// don't quite yet have a distiction for in JGit.
	//
	public class RevWalkPathFilter6012Test : RevWalkTestCase
	{
		private const string pA = "pA", pE = "pE", pF = "pF";
		private RevCommit a, b, c, d, e, f, g, h, i;
		private Dictionary<RevCommit, string> byName;

		protected override void SetUp()
		{
			base.SetUp();

			// Test graph was stolen from git-core t6012-rev-list-simplify
			// (by Junio C Hamano in 65347030590bcc251a9ff2ed96487a0f1b9e9fa8)
			//
			RevBlob zF = Blob("zF");
			RevBlob zH = Blob("zH");
			RevBlob zI = Blob("zI");
			RevBlob zS = Blob("zS");
			RevBlob zY = Blob("zY");

			a = Commit(Tree(File(pF, zH)));
			b = Commit(Tree(File(pF, zI)), a);
			c = Commit(Tree(File(pF, zI)), a);
			d = Commit(Tree(File(pA, zS), File(pF, zI)), c);
			Parse(d);

			e = Commit(d.Tree, d, b);
			f = Commit(Tree(File(pA, zS), File(pE, zY), File(pF, zI)), e);
			Parse(f);

			g = Commit(Tree(File(pE, zY), File(pF, zI)), b);
			h = Commit(f.Tree, g, f);
			i = Commit(Tree(File(pA, zS), File(pE, zY), File(pF, zF)), h);

			byName = new Dictionary<RevCommit, string>();
			var fields = typeof(RevWalkPathFilter6012Test).GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
				.Where(x => x.FieldType == typeof(RevCommit));

			foreach (FieldInfo z in fields)
			{
				byName.Add((RevCommit)z.GetValue(this), z.Name);
			}
		}

		private void Check(params RevCommit[] order)
		{
			MarkStart(i);

			var act = new StringBuilder();
			foreach (RevCommit z in Rw)
			{
				string name = byName[z];
				Assert.NotNull(name);
				act.Append(name);
				act.Append(' ');
			}

			var exp = new StringBuilder();
			foreach (RevCommit z in order)
			{
				string name = byName[z];
				Assert.NotNull(name);
				exp.Append(name);
				exp.Append(' ');
			}

			Assert.Equal(exp.ToString(), act.ToString());
		}

		private void Filter(string path)
		{
			Rw.setTreeFilter(AndTreeFilter.create(PathFilterGroup.createFromStrings(Enumerable.Repeat(path, 1)), TreeFilter.ANY_DIFF));
		}

		[StrictFactAttribute]
		public void test1()
		{
			// TODO --full-history
			Check(i, h, g, f, e, d, c, b, a);
		}

		[StrictFactAttribute]
		public void test2()
		{
			// TODO --full-history
			Filter(pF);
			// TODO fix broken test
			//Check(i, h, e, c, b, a);
		}

		[StrictFactAttribute]
		public void test3()
		{
			// TODO --full-history
			Rw.sort(RevSort.TOPO);
			Filter(pF);
			// TODO fix broken test
			//Check(i, h, e, c, b, a);
		}

		[StrictFactAttribute]
		public void test4()
		{
			// TODO --full-history
			Rw.sort(RevSort.COMMIT_TIME_DESC);
			Filter(pF);
			// TODO fix broken test
			//Check(i, h, e, c, b, a);
		}

		[StrictFactAttribute]
		public void test5()
		{
			// TODO --simplify-merges
			Filter(pF);
			// TODO fix broken test
			//Check(i, e, c, b, a);
		}

		[StrictFactAttribute]
		public void test6()
		{
			Filter(pF);
			Check(i, b, a);
		}

		[StrictFactAttribute]
		public void test7()
		{
			Rw.sort(RevSort.TOPO);
			Filter(pF);
			Check(i, b, a);
		}
	}
}