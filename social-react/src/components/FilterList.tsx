import * as React from 'react';
import * as FilterListStore from '../store/FilterList'
import { ApplicationState } from '../store';
import { connect } from 'react-redux';

type FilterListProps = FilterListStore.FilterListState & typeof FilterListStore.actionCreators

class FilterList extends React.Component<FilterListProps, {}>{

    render() {
        return <div>
            <div>Filters</div>
            <div>
                <ul>
                    {this.props.list.map(filter => <li key={filter.id}>{filter.name} - {filter.unReadNum}</li>)}
                </ul>
            </div>
        </div>
    }
}

// Wire up the React component to the Redux store
export default connect(
    (state: ApplicationState) => state.FilterList, // Selects which state properties are merged into the component's props
    FilterListStore.actionCreators                 // Selects which action creators are merged into the component's props
)(FilterList) as typeof FilterList;