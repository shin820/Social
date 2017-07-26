import FilterList from '../components/FilterList/FilterList';
import * as actions from '../actions/';
import { ApplicationState } from '../types/index';
import { connect, Dispatch } from 'react-redux';

export function mapStateToProps({ FilterList }: ApplicationState) {
  return {
    list: FilterList
  }
}

export function mapDispatchToProps(dispatch: Dispatch<actions.FilterAction>) {
  return {
    onReload: () => dispatch(actions.ReloadFilterList()),
  }
}

export default connect(mapStateToProps, mapDispatchToProps)(FilterList);