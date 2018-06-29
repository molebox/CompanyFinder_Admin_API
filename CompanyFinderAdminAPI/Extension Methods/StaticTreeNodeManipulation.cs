using CompanyDatabase.Models;
using CompanyFinderAdminAPI.BindingModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompanyFinderAdminAPI.Extension_Methods
{
    /// <summary>
    /// Static helper methods for manipulating the tree nodes
    /// </summary>
    public class StaticTreeNodeManipulation
    {
        /// <summary>
        /// Save tree nodes with state
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public static List<TreeNodesWithState> SaveAllNodesWithState(List<TreeNodesWithState> nodes, int parentId)
        {
            return nodes.Where(l => l.ParentId == parentId).OrderBy(l => l.OrderNumber)
                .Select(l => new TreeNodesWithState
                {
                    Id = l.Id,
                    Text = l.Text,
                    ParentId = l.ParentId,
                    OrderNumber = l.OrderNumber,
                    State = l.State,
                    Children = SaveAllNodesWithState(nodes, l.Id)
                }).ToList();
        }
        /// <summary>
        /// Save focus nodes with state
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public static List<FocusNodesWithState> SaveAllFocusNodesWithState(List<FocusNodesWithState> nodes, int parentId)
        {
            return nodes.Where(l => l.ParentId == parentId).OrderBy(l => l.OrderNumber)
                .Select(l => new FocusNodesWithState
                {
                    Id = l.Id,
                    Text = l.Text,
                    ParentId = l.ParentId,
                    OrderNumber = l.OrderNumber,
                    State = l.State,
                    Children = SaveAllFocusNodesWithState(nodes, l.Id)
                }).ToList();
        }
        /// <summary>
        /// Save focus nodes
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public static List<FocusNodes> SaveAllFocusNodes(List<FocusNodes> nodes, int parentId)
        {
            return nodes.Where(l => l.ParentId == parentId).OrderBy(l => l.OrderNumber)
                .Select(l => new FocusNodes
                {
                    Name = l.Name,
                    Id = l.Id,
                    ParentId = l.ParentId,
                    OrderNumber = l.OrderNumber,
                    Children = SaveAllFocusNodes(nodes, l.Id)
                }).ToList();
        }
        /// <summary>
        /// Create tree with state
        /// </summary>
        /// <param name="tempTreeNodeList"></param>
        /// <returns></returns>
        public static List<TreeNodesWithState> CreateTreeWithState(List<TreeNodesWithState> tempTreeNodeList)
        {
            return tempTreeNodeList.Where(l => l.ParentId == null)
                .Select(l => new TreeNodesWithState
                {
                    Id = l.Id,
                    Text = l.Text,
                    ParentId = l.ParentId,
                    OrderNumber = l.OrderNumber,
                    State = l.State,
                    Children = SaveAllNodesWithState(tempTreeNodeList, l.Id)
                }).ToList();
        }
        /// <summary>
        /// Create focus tree with state
        /// </summary>
        /// <param name="tempFocusNodeList"></param>
        /// <returns></returns>
        public static List<FocusNodesWithState> CreateFocusTreeWithState(List<FocusNodesWithState> tempFocusNodeList)
        {
            return tempFocusNodeList.Where(l => l.ParentId == null)
                .Select(l => new FocusNodesWithState
                {
                    Id = l.Id,
                    Text = l.Text,
                    ParentId = l.ParentId,
                    OrderNumber = l.OrderNumber,
                    State = l.State,
                    Children = SaveAllFocusNodesWithState(tempFocusNodeList, l.Id)
                }).ToList();
        }
    }
}
