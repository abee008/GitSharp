﻿/*
 * Copyright (C) 2009, Google Inc.
 * Copyright (C) 2009, Gil Ran <gilrun@gmail.com>
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

using GitSharp.Diff;
using Xunit;

namespace GitSharp.Tests.Diff
{
	public class EditTest
	{
		[StrictFactAttribute]
		public void testCreate()
		{
			var e = new Edit(1, 2, 3, 4);
			Assert.Equal(1, e.BeginA);
			Assert.Equal(2, e.EndA);
			Assert.Equal(3, e.BeginB);
			Assert.Equal(4, e.EndB);
		}

		[StrictFactAttribute]
		public void testCreateEmpty()
		{
			var e = new Edit(1, 3);
			Assert.Equal(1, e.BeginA);
			Assert.Equal(1, e.EndA);
			Assert.Equal(3, e.BeginB);
			Assert.Equal(3, e.EndB);
		}

		[StrictFactAttribute]
		public void testSwap()
		{
			var e = new Edit(1, 2, 3, 4);
			e.Swap();
			Assert.Equal(3, e.BeginA);
			Assert.Equal(4, e.EndA);
			Assert.Equal(1, e.BeginB);
			Assert.Equal(2, e.EndB);
		}

		[StrictFactAttribute]
		public void testType_Insert()
		{
			var e = new Edit(1, 1, 1, 2);
			Assert.Equal(Edit.Type.INSERT, e.EditType);
		}

		[StrictFactAttribute]
		public void testType_Delete()
		{
			var e = new Edit(1, 2, 1, 1);
			Assert.Equal(Edit.Type.DELETE, e.EditType);
		}

		[StrictFactAttribute]
		public void testType_Replace()
		{
			var e = new Edit(1, 2, 1, 4);
			Assert.Equal(Edit.Type.REPLACE, e.EditType);
		}

		[StrictFactAttribute]
		public void testType_Empty() 
		{
			Assert.Equal(Edit.Type.EMPTY, new Edit(1, 1, 2, 2).EditType);
			Assert.Equal(Edit.Type.EMPTY, new Edit(1, 2).EditType);
		}

		[StrictFactAttribute]
		public void testToString()
		{
			var e = new Edit(1, 2, 1, 4);
			Assert.Equal("REPLACE(1-2,1-4)", e.ToString());
		}

		[StrictFactAttribute]
		public void testEquals1()
		{
			var e1 = new Edit(1, 2, 3, 4);
			var e2 = new Edit(1, 2, 3, 4);

			Assert.True(e1.Equals(e1));
			Assert.True(e1.Equals(e2));
			Assert.True(e2.Equals(e1));
			Assert.Equal(e1.GetHashCode(), e2.GetHashCode());
			Assert.False(e1.Equals(""));
		}

		[StrictFactAttribute]
		public void testNotEquals1()
		{
			Assert.False(new Edit(1, 2, 3, 4).Equals(new Edit(0, 2, 3, 4)));
		}

		[StrictFactAttribute]
		public void testNotEquals2()
		{
			Assert.False(new Edit(1, 2, 3, 4).Equals(new Edit(1, 0, 3, 4)));
		}

		[StrictFactAttribute]
		public void testNotEquals3()
		{
			Assert.False(new Edit(1, 2, 3, 4).Equals(new Edit(1, 2, 0, 4)));
		}

		[StrictFactAttribute]
		public void testNotEquals4()
		{
			Assert.False(new Edit(1, 2, 3, 4).Equals(new Edit(1, 2, 3, 0)));
		}

		[StrictFactAttribute]
		public void testExtendA()
		{
			var e = new Edit(1, 2, 1, 1);

			e.ExtendA();
			Assert.Equal(new Edit(1, 3, 1, 1), e);

			e.ExtendA();
			Assert.Equal(new Edit(1, 4, 1, 1), e);
		}

		[StrictFactAttribute]
		public void testExtendB()
		{
			var e = new Edit(1, 2, 1, 1);

			e.ExtendB();
			Assert.Equal(new Edit(1, 2, 1, 2), e);

			e.ExtendB();
			Assert.Equal(new Edit(1, 2, 1, 3), e);
		}
	}
}